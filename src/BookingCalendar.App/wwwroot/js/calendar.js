window.calendar = {
  _media: null,
  _onMedia: null,
  _month: null,
  _traps: {},

  load: function () {
    return localStorage.getItem("calendar.appointments.v1");
  },
  save: function (json) {
    localStorage.setItem("calendar.appointments.v1", json);
  },
  clear: function () {
    localStorage.removeItem("calendar.appointments.v1");
  },
  loadTheme: function () {
    return localStorage.getItem("calendar.theme.v1");
  },
  saveTheme: function (name) {
    localStorage.setItem("calendar.theme.v1", name);
  },
  applyTheme: function (name) {
    document.documentElement.dataset.theme = name === "dark" ? "dark" : "light";
  },
  prefersDark: function () {
    return window.matchMedia("(prefers-color-scheme: dark)").matches;
  },
  loadClock: function () {
    return localStorage.getItem("calendar.clock.v1");
  },
  saveClock: function (name) {
    localStorage.setItem("calendar.clock.v1", name);
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
  anchor: function (iso) {
    var node = document.querySelector("[data-month-grid] [data-date='" + iso + "']");
    if (!node) {
      return null;
    }
    var rect = node.getBoundingClientRect();
    return {
      top: rect.top,
      left: rect.left,
      right: rect.right,
      bottom: rect.bottom,
      viewWidth: window.innerWidth,
      viewHeight: window.innerHeight
    };
  },
  bindTrap: function (id, ref) {
    this.unbindTrap(id);
    var handler = function (event) {
      var root = document.querySelector("[data-trap='" + id + "']");
      if (!root) {
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
      var nodes = root.querySelectorAll("button, [href], input, select, textarea");
      var list = Array.prototype.filter.call(nodes, function (node) {
        return !node.disabled && node.tabIndex !== -1;
      });
      if (!list.length) {
        return;
      }
      var first = list[0];
      var last = list[list.length - 1];
      var inside = root.contains(document.activeElement);
      if (!inside) {
        event.preventDefault();
        (event.shiftKey ? last : first).focus();
        return;
      }
      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault();
        first.focus();
      }
    };
    this._traps[id] = handler;
    document.addEventListener("keydown", handler);
  },
  unbindTrap: function (id) {
    var handler = this._traps[id];
    if (handler) {
      document.removeEventListener("keydown", handler);
      delete this._traps[id];
    }
  }
};
