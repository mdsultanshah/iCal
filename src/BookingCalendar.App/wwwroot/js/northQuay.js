window.northQuay = {
  load: function () {
    return localStorage.getItem("northquay.bookings.v1");
  },
  save: function (json) {
    localStorage.setItem("northquay.bookings.v1", json);
  },
  clear: function () {
    localStorage.removeItem("northquay.bookings.v1");
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
  copy: function (text) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      return navigator.clipboard.writeText(text);
    }
    var area = document.createElement("textarea");
    area.value = text;
    document.body.appendChild(area);
    area.select();
    document.execCommand("copy");
    document.body.removeChild(area);
    return Promise.resolve();
  }
};
