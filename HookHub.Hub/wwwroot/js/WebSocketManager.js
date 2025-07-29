var HOOKHUBNETManager = (function () {
    var constructor = function (url) {
        if (url === undefined) console.error("HookHubNet necesita una 'url' válida.");
        _this = this;



        var MensajeWeb = function (claveSesion, tipoMensajeWeb, datos, claveUsuarioOrigen, claveUsuarioDestino) {
            this.ClaveSesion = claveSesion;
            this.TipoMensajeWeb = tipoMensajeWeb;
            this.Datos = datos;
            //this.MarcaDeTiempo = ;
            this.HookNameFrom = claveUsuarioOrigen;
            this.HookNameTo = claveUsuarioDestino;
        };



        /********************************************************/
        /********* Mapear de la clase MensajeWeb.cs *************/
        /********************************************************/
        MensajeWeb.Conexion = 0;
        MensajeWeb.Difusion = 1;
        MensajeWeb.Texto = 2;
        MensajeWeb.Consulta = 3;
        MensajeWeb.Respuesta = 4;
        MensajeWeb.ErrorLocal = 5;
        MensajeWeb.ErrorRemoto = 6;
        MensajeWeb.ErrorGeneral = 7;
        /********************************************************/
        /********************************************************/
        /********************************************************/



        var typemappings = {
            guid: 'System.Guid',
            uuid: 'System.Guid',
            bool: 'System.Boolean',
            byte: 'System.Byte',
            sbyte: 'System.SByte',
            char: 'System.Char',
            decimal: 'System.Decimal',
            double: 'System.Double',
            float: 'System.Single',
            int: 'System.Int32',
            uint: 'System.UInt32',
            long: 'System.Int64',
            ulong: 'System.UInt64',
            short: 'System.Int16',
            ushort: 'System.UInt16',
            string: 'System.String',
            object: 'System.Object'
        };



        var onSocketOpen = function (event) {
        };



        var onSocketClose = function (event) {
            if (_this.onDisconnected !== undefined) _this.onDisconnected();
        };



        var onSocketError = function (event) {
            console.error("HOOKHUBNET error:");
            console.error(event);
        };



        var onSocketMessage = function (message) {



            _this.TipoMensajeWeb = message.TipoMensajeWeb;
            _this.Datos = message.Datos;
            _this.HookNameFrom = message.HookNameFrom;



            if (message.TipoMensajeWeb === MensajeWeb.Conexion) {
                _this.ClaveSesion = message.Datos;
                if (_this.onConnected !== undefined) _this.onConnected(_this);



                var datos = "Hola HookHubNet";
                var claveSesion = message.Datos;
                var tipoMensajeWeb = MensajeWeb.Conexion;
                var claveUsuarioOrigen = message.HookNameTo;
                var claveUsuarioDestino = message.HookNameFrom;
                var unMensaje = new MensajeWeb(claveSesion, tipoMensajeWeb, datos, claveUsuarioOrigen, claveUsuarioDestino);
                _this.socket.send(JSON.stringify(unMensaje));
            }
            else if (message.TipoMensajeWeb === MensajeWeb.Texto) {
                _this.HookNameTo = message.HookNameTo;
                if (_this.onMessage !== undefined) _this.onMessage(_this);
            }
        };



        this.connect = function () {
            _this.socket = new WebSocket(url);



            _this.socket.onopen = function (event) {
                onSocketOpen(event);
            };



            _this.socket.onclose = function (event) {
                onSocketClose(event);
            };



            _this.socket.onerror = function (event) {
                onSocketError(event);
            };



            _this.socket.onmessage = function (event) {
                onSocketMessage(JSON.parse(event.data));
            };
        };



        this.sendMessage = function (mensaje, claveUsuarioOrigen, claveUsuarioDestino) {
            var datos = mensaje;
            var claveSesion = this.ClaveSesion;
            var tipoMensajeWeb = MensajeWeb.Texto;
            //var claveUsuarioOrigen = this.HookNameFrom;
            //var claveUsuarioDestino = this.HookNameTo;
            var unMensaje = new MensajeWeb(claveSesion, tipoMensajeWeb, datos, claveUsuarioOrigen, claveUsuarioDestino);
            _this.socket.send(JSON.stringify(unMensaje));
        };



    };



    return constructor;
})();