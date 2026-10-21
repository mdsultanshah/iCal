window.northQuay = {
  _media: null,
  _onMedia: null,
  _dialogs: {},

  load: function () {
    return localStorage.getItem("northquay.appointments.v1");
  },
  save: function (json) {
    localStorage.setItem("northquay.appointments.v1", json);
  },
  clear: function () {
    localStorage.removeItem("northquay.appointments.v1");
  },
  zone: function () {
    var timeZone = "UTC";
    try {
      timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC";
    } catch (error) {
      timeZone = "UTC";
    }
    return {
      timeZone: timeZone,
      offsetMinutes: -new Date().getTimezoneOffset()
    };
  },
  isNarrow: function () {
    return window.matchMedia("(max-width: 799px)").matches;
  },
  watchNarrow: function (ref) {
    this.unwatchNarrow();
    var media = window.matchMedia("(max-width: 799px)");
    var onChange = function (event) {
      ref.invokeMethodAsync("SetNarrow", event.matches);
    };
    media.addEventListener("change", onChange);
    this._media = media;
    this._onMedia = onChange;
  },
  unwatchNarrow: function () {
    if (this._media && this._onMedia) {
      this._media.removeEventListener("change", this._onMedia);
    }
    this._media = null;
    this._onMedia = null;
  },
  focus: function (selector) {
    var node = document.querySelector(selector);
    if (node && node.focus) {
      node.focus();
    }
  },
  bindDialog: function (id, ref) {
    this.unbindDialog(id);
    var handler = function (event) {
      var dialog = document.querySelector("[data-dialog]");
      if (!dialog) {
        return;
      }
      if (event.key === "Escape") {
        event.preventDefault();
        ref.invokeMethodAsync("CloseFromKey");
        return;
      }
      if (event.key !== "Tab") {
        return;
      }
      var nodes = dialog.querySelectorAll("button, [href], input, select, textarea");
      var list = Array.prototype.filter.call(nodes, function (node) {
        return !node.disabled && node.tabIndex !== -1;
      });
      if (!list.length) {
        return;
      }
      var first = list[0];
      var last = list[list.length - 1];
      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault();
        first.focus();
      }
    };
    this._dialogs[id] = handler;
    document.addEventListener("keydown", handler);
  },
  unbindDialog: function (id) {
    var handler = this._dialogs[id];
    if (handler) {
      document.removeEventListener("keydown", handler);
      delete this._dialogs[id];
    }
  }
};
