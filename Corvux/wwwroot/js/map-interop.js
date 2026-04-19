// map-interop.js — JSInterop para Leaflet.js
// Expone window.corvuxMap = { init, updateMarkers, destroy }

window.corvuxMap = {
    _map: null,
    _markers: [],

    init: function (containerId) {
        if (this._map) {
            this._map.remove();
        }

        this._map = L.map(containerId).setView([20, 0], 2);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 18
        }).addTo(this._map);
    },

    updateMarkers: function (points) {
        if (!this._map) return;

        // Limpiar marcadores anteriores
        this._markers.forEach(m => this._map.removeLayer(m));
        this._markers = [];

        points.forEach(point => {
            var marker = L.circleMarker([point.lat, point.lng], {
                radius: 8,
                fillColor: point.color,
                color: point.color,
                weight: 2,
                opacity: 0.8,
                fillOpacity: 0.6
            }).addTo(this._map);

            marker.bindPopup(
                '<strong>' + point.country + '</strong><br/>' + point.label
            );

            this._markers.push(marker);
        });

        // Ajustar vista si hay marcadores
        if (this._markers.length > 0) {
            var group = L.featureGroup(this._markers);
            this._map.fitBounds(group.getBounds().pad(0.2));
        }
    },

    destroy: function () {
        if (this._map) {
            this._map.remove();
            this._map = null;
            this._markers = [];
        }
    }
};

// Función para descarga de archivos (blob download)
window.downloadFile = function (filename, base64Content) {
    var byteCharacters = atob(base64Content);
    var byteNumbers = new Array(byteCharacters.length);
    for (var i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    var byteArray = new Uint8Array(byteNumbers);
    var blob = new Blob([byteArray], { type: 'text/csv' });

    var link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(link.href);
};
