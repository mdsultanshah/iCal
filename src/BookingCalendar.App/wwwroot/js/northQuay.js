window.northQuay = {
  _media: null,
  _onMedia: null,
  _month: null,
  _escape: null,

  load: function () {
    return localStorage.getItem("northquay.appointments.v1");
  },
  save: function (json) {
    localStorage.setItem("northquay.appointments.v1", json);
  },
  clear: function () {
    localStorage.removeItem("northquay.appointments.v1");
  },
  loadTheme: function () {
    return localStorage.getItem("northquay.theme.v1");
  },
  saveTheme: function (name) {
    localStorage.setItem("northquay.theme.v1", name);
  },
  applyTheme: function (name) {
    document.documentElement.dataset.theme = name === "dark" ? "dark" : "light";
  },
  prefersDark: function () {
    return window.matchMedia("(prefers-color-scheme: dark)").matches;
  },
  loadClock: function () {
    return localStorage.getItem("northquay.clock.v1");
  },
  saveClock: function (name) {
    localStorage.setItem("northquay.clock.v1", name);
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
  bindMonth: function (ref) {
    this.unbindMonth();
    var handler = function (event) {
      var key = event.key;
      if (key !== "ArrowLeft" && key !== "ArrowRight" && key !== "ArrowUp" && key !== "ArrowDown") {
        return;
      }
      var target = event.target;
      if (!target || !target.closest) {
        return;
      }
      if (target.closest("input, textarea, select")) {
        return;
      }
      var button = target.closest("[data-month-grid] [data-date]");
      if (!button) {
        return;
      }
      event.preventDefault();
      ref.invokeMethodAsync("MoveDay", button.getAttribute("data-date"), key);
    };
    this._month = handler;
    document.addEventListener("keydown", handler);
  },
  unbindMonth: function () {
    if (this._month) {
      document.removeEventListener("keydown", this._month);
      this._month = null;
    }
  },
  bindEscape: function (ref) {
    this.unbindEscape();
    var handler = function (event) {
      if (event.key !== "Escape") {
        return;
      }
      var target = event.target;
      if (target && target.closest && target.closest("[data-month-grid]")) {
        return;
      }
      event.preventDefault();
      ref.invokeMethodAsync("CloseFromKey");
    };
    this._escape = handler;
    document.addEventListener("keydown", handler);
  },
  unbindEscape: function () {
    if (this._escape) {
      document.removeEventListener("keydown", this._escape);
      this._escape = null;
    }
  }
};
