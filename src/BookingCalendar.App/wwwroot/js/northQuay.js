window.northQuay = {
  load: function () {
    return localStorage.getItem("northquay.bookings.v1");
  },
  save: function (json) {
    localStorage.setItem("northquay.bookings.v1", json);
  },
  clear: function () {
    localStorage.removeItem("northquay.bookings.v1");
  }
};
