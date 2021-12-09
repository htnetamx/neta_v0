﻿/*
** amplitude implementation v 1.0.0
*/

(function (e, t) {
  var r = e.amplitude || { _q: [], _iq: {} }; var n = t.createElement("script")
    ; n.type = "text/javascript"
    ; n.integrity = "sha384-4rr7CTymHc64YjTTL6O3ktfsHYI1yJnQdmKv4zFoe+frjXb05MfzzuLLIAgJ/XHs"
    ; n.crossOrigin = "anonymous"; n.async = true
    ; n.src = "https://cdn.amplitude.com/libs/amplitude-8.11.0-min.gz.js"
    ; n.onload = function () {
      if (!e.amplitude.runQueuedFunctions) {
        console.log("[Amplitude] Error: could not load SDK")
      }
    }
    ; var s = t.getElementsByTagName("script")[0]; s.parentNode.insertBefore(n, s)
    ; function i(e, t) {
      e.prototype[t] = function () {
        this._q.push([t].concat(Array.prototype.slice.call(arguments, 0))); return this
      }
    }
  var o = function () { this._q = []; return this }
    ; var a = ["add", "append", "clearAll", "prepend", "set", "setOnce", "unset", "preInsert", "postInsert", "remove"]
    ; for (var c = 0; c < a.length; c++) { i(o, a[c]) } r.Identify = o; var u = function () {
      this._q = []
      ; return this
    }
    ; var p = ["setProductId", "setQuantity", "setPrice", "setRevenueType", "setEventProperties"]
    ; for (var l = 0; l < p.length; l++) { i(u, p[l]) } r.Revenue = u
    ; var d = ["init", "logEvent", "logRevenue", "setUserId", "setUserProperties", "setOptOut", "setVersionName", "setDomain", "setDeviceId", "enableTracking", "setGlobalUserProperties", "identify", "clearUserProperties", "setGroup", "logRevenueV2", "regenerateDeviceId", "groupIdentify", "onInit", "logEventWithTimestamp", "logEventWithGroups", "setSessionId", "resetSessionId", "setLibrary", "setTransport"]
    ; function v(e) {
      function t(t) {
        e[t] = function () {
          e._q.push([t].concat(Array.prototype.slice.call(arguments, 0)))
        }
      }
      for (var r = 0; r < d.length; r++) { t(d[r]) }
    } v(r); r.getInstance = function (e) {
      e = (!e || e.length === 0 ? "$default_instance" : e).toLowerCase()
        ; if (!Object.prototype.hasOwnProperty.call(r._iq, e)) {
          r._iq[e] = { _q: [] }; v(r._iq[e])
        } return r._iq[e]
    }; e.amplitude = r
})(window, document);

document.addEventListener("DOMContentLoaded", function (event) {
  //settings main options
  var options = {
    includeGclid: true,
    includeFbclid: true,
    includeReferrer: true,
    includeUtm: true,
    unsetParamsReferrerOnNewSession: true
  };

  //initializing
  amplitude.getInstance().init("f5232ee25585b5bb455a9a3710c685e6", options);
});