using System;
using System.ComponentModel.DataAnnotations;

namespace HookHub.Web.Models
{
    [Serializable]
    public class FacturaTransporteFleteraModelBase
    {
        #region private
        private string _version;
        private string _transporteInternacional;
        private string _entradaSalida;
        private string _viaEntradaSalida;
        private string _totalDistanciaRecorrida;
        private string _remitenteRFC;
        private string _remitenteNombre;
        private string _origenTipoUbicacion;
        private string _origenIDUbicacion;
        private string _origenRFC;
        private string _origenNombre;
        private string _origenRegistroFiscal;
        private string _origenResidenciaFiscal;
        private DateTime? _origenFechaHoraSalida;
        private string _origenDomicilioCalle;
        private string _origenDomicilioNumeroExterior;
        private string _origenDomicilioNumeroInterior;
        private string _origenDomicilioColonia;
        private string _origenDomicilioLocalidad;
        private string _origenDomicilioReferencia;
        private string _origenDomicilioMunicipo;
        private string _origenDomicilioEstado;
        private string _origenDomicilioPais;
        private string _origenDomicilioCodigoPostal;
        private string _destinoRFC;
        private string _destinoNombre;
        private string _destinoRegistroFiscal;
        private string _destinoResidenciaFiscal;
        private DateTime? _destinoFechaHoraLlegada;
        private string _destinoDomicilioCalle;
        private string _destinoDomicilioNumeroExterior;
        private string _destinoDomicilioNumeroInterior;
        private string _destinoDomicilioColonia;
        private string _destinoDomicilioLocalidad;
        private string _destinoDomicilioReferencia;
        private string _destinoDomicilioMunicipo;
        private string _destinoDomicilioEstado;
        private string _destinoDomicilioPais;
        private string _destinoDomicilioCodigoPostal;
        private string _mercanciasPesoBrutoTotal;
        private string _mercanciasUnidadPeso;
        private string _mercanciasPesoNetoTotal;
        private string _mercanciasNumeroTotal;
        private string _mercanciaBienesTransportados;
        private string _mercanciaDescripcion;
        private string _mercanciaCantidad;
        private string _mercanciaClaveUnidad;
        private string _mercanciaUnidad;
        private string _mercanciaDimensiones;
        private string _mercanciaMaterialPeligroso;
        private string _mercanciaClaveMarterialPeligroso;
        private string _mercanciaEmbalaje;
        private string _mercanciaDescripcionEmbalaje;
        private string _mercanciaPesoKilogramo;
        private string _mercanciaValor;
        private string _mercanciaMoneda;
        private string _mercanciaFraccionArancelaria;
        private string _cantidadTransportada;
        private string _cantidadTransportadaOrigenID;
        private string _cantidadTransportadaDestinoID;
        private string _federalPermisoSCT;
        private string _federalNumeroPermisoSCT;
        private string _federalNombreAseguradora;
        private string _federalNumeroPolizaSeguro;
        private string _idVehicularConfiguracion;
        private string _idVehicularPlacaVehiculoMotor;
        private string _idVehicularAnioModeloVM;
        private string _remolqueSubtipo;
        private string _remolquePlaca;
        private string _serieBitacora;
        private string _serieRemision;
        private string _operadorRFC;
        private string _operadorNumLicencia;
        private string _operadorNombre;
        private string _operadorRegistroFiscal;
        private string _operadorResidenciaFiscal;
        private string _paisOrigenDestino;
        private string _distanciaRecorrida;
        private string _destinoTipoUbicacion;
        private string _destinoIDUbicacion;
        private string _mercanciaPedimiento;
        private string _tipoFigura;
        private string _parteTransporte;
        private DateTime? _federalFechaInicialPoliza;
        private DateTime? _federalFechaFinalPoliza;
        private string _seguroFederalAseguraMedAmbiente;
        private string _seguroFederalNumeroPolizaAseguraMedAmbiente;
        private string _seguroAseguraCarga;
        private string _seguroPrimaSeguro;
        private string _claveTipoUnidad;
        private string _tipoUnidad;
        private string _mercanciaUUIDComprobanteComercioExterior;
        #endregion private

        [Required]
        [StringLength(2, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string SerieBitacora { get { return (_serieBitacora ?? ""); } set { _serieBitacora = value; } }

        [Required]
        public int FolioBitacora { get; set; }

        public bool BitacoraPrimaria { get; set; }

        public int FolioBitacoraSecundaria { get; set; }

        [Required]
        [StringLength(2, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string SerieRemision { get { return (_serieRemision ?? ""); } set { _serieRemision = value; } }

        [Required]
        public int FolioRemision { get; set; }

        #region general

        [StringLength(8, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string Version { get { return (_version ?? ""); } set { _version = value; } }

        [StringLength(4, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string TransporteInternacional { get { return (_transporteInternacional ?? ""); } set { _transporteInternacional = value; } }

        [StringLength(8, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string EntradaSalida { get { return (_entradaSalida ?? ""); } set { _entradaSalida = value; } }

        [StringLength(8, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string PaisOrigenDestino { get { return (_paisOrigenDestino ?? ""); } set { _paisOrigenDestino = value; } }

        [StringLength(8, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string ViaEntradaSalida { get { return (_viaEntradaSalida ?? ""); } set { _viaEntradaSalida = value; } }

        [StringLength(32, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string TotalDistanciaRecorrida { get { return (_totalDistanciaRecorrida ?? "0.0"); } set { _totalDistanciaRecorrida = value; } }
        #endregion general

        #region remitente

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string RemitenteRFC { get { return (_remitenteRFC ?? ""); } set { _remitenteRFC = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string RemitenteNombre { get { return (_remitenteNombre ?? ""); } set { _remitenteNombre = value; } }

        #endregion remitente

        #region remitente

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenTipoUbicacion { get { return (_origenTipoUbicacion ?? ""); } set { _origenTipoUbicacion = value; } }

        [StringLength(1024, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenIDUbicacion { get { return (_origenIDUbicacion ?? ""); } set { _origenIDUbicacion = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenRFC { get { return (_origenRFC ?? ""); } set { _origenRFC = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenNombre { get { return (_origenNombre ?? ""); } set { _origenNombre = value; } }

        [StringLength(512, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenRegistroFiscal { get { return (_origenRegistroFiscal ?? ""); } set { _origenRegistroFiscal = value; } }

        [StringLength(1024, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenResidenciaFiscal { get { return (_origenResidenciaFiscal ?? ""); } set { _origenResidenciaFiscal = value; } }

        public DateTime? OrigenFechaHoraSalida { get { return (_origenFechaHoraSalida); } set { _origenFechaHoraSalida = value; } }

        #region domicilio
        
        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenDomicilioCalle { get { return (_origenDomicilioCalle ?? ""); } set { _origenDomicilioCalle = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenDomicilioNumeroExterior { get { return (_origenDomicilioNumeroExterior ?? ""); } set { _origenDomicilioNumeroExterior = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenDomicilioNumeroInterior { get { return (_origenDomicilioNumeroInterior ?? ""); } set { _origenDomicilioNumeroInterior = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenDomicilioColonia { get { return (_origenDomicilioColonia ?? ""); } set { _origenDomicilioColonia = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenDomicilioLocalidad { get { return (_origenDomicilioLocalidad ?? ""); } set { _origenDomicilioLocalidad = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenDomicilioReferencia { get { return (_origenDomicilioReferencia ?? ""); } set { _origenDomicilioReferencia = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenDomicilioMunicipo { get { return (_origenDomicilioMunicipo ?? ""); } set { _origenDomicilioMunicipo = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenDomicilioEstado { get { return (_origenDomicilioEstado ?? ""); } set { _origenDomicilioEstado = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenDomicilioPais { get { return (_origenDomicilioPais ?? ""); } set { _origenDomicilioPais = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OrigenDomicilioCodigoPostal { get { return (_origenDomicilioCodigoPostal ?? ""); } set { _origenDomicilioCodigoPostal = value; } }

        #endregion domicilio

        #endregion remitente

        #region distancia

        [StringLength(32, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DistanciaRecorrida { get { return (_distanciaRecorrida ?? "0.0"); } set { _distanciaRecorrida = value; } }

        #endregion distancia

        #region destino

        [StringLength(8, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoTipoUbicacion { get { return (_destinoTipoUbicacion ?? ""); } set { _destinoTipoUbicacion = value; } }

        [StringLength(1024, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoIDUbicacion { get { return (_destinoIDUbicacion ?? ""); } set { _destinoIDUbicacion = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoRFC { get { return (_destinoRFC ?? ""); } set { _destinoRFC = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoNombre { get { return (_destinoNombre ?? ""); } set { _destinoNombre = value; } }

        [StringLength(512, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoRegistroFiscal { get { return (_destinoRegistroFiscal ?? ""); } set { _destinoRegistroFiscal = value; } }

        [StringLength(1024, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoResidenciaFiscal { get { return (_destinoResidenciaFiscal ?? ""); } set { _destinoResidenciaFiscal = value; } }

        public DateTime? DestinoFechaHoraLlegada { get { return (_destinoFechaHoraLlegada); } set { _destinoFechaHoraLlegada = value; } }

        #region domicilio

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoDomicilioCalle { get { return (_destinoDomicilioCalle ?? ""); } set { _destinoDomicilioCalle = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoDomicilioNumeroExterior { get { return (_destinoDomicilioNumeroExterior ?? ""); } set { _destinoDomicilioNumeroExterior = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoDomicilioNumeroInterior { get { return (_destinoDomicilioNumeroInterior ?? ""); } set { _destinoDomicilioNumeroInterior = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoDomicilioColonia { get { return (_destinoDomicilioColonia ?? ""); } set { _destinoDomicilioColonia = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoDomicilioLocalidad { get { return (_destinoDomicilioLocalidad ?? ""); } set { _destinoDomicilioLocalidad = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoDomicilioReferencia { get { return (_destinoDomicilioReferencia ?? ""); } set { _destinoDomicilioReferencia = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoDomicilioMunicipo { get { return (_destinoDomicilioMunicipo ?? ""); } set { _destinoDomicilioMunicipo = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoDomicilioEstado { get { return (_destinoDomicilioEstado ?? ""); } set { _destinoDomicilioEstado = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoDomicilioPais { get { return (_destinoDomicilioPais ?? ""); } set { _destinoDomicilioPais = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string DestinoDomicilioCodigoPostal { get { return (_destinoDomicilioCodigoPostal ?? ""); } set { _destinoDomicilioCodigoPostal = value; } }

        #endregion domicilio

        #endregion destino

        #region mercancias
        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciasPesoBrutoTotal { get { return (_mercanciasPesoBrutoTotal ?? ""); } set { _mercanciasPesoBrutoTotal = value; } }

        [StringLength(25, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciasUnidadPeso { get { return (_mercanciasUnidadPeso ?? ""); } set { _mercanciasUnidadPeso = value; } }

        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciasPesoNetoTotal { get { return (_mercanciasPesoNetoTotal ?? ""); } set { _mercanciasPesoNetoTotal = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciasNumeroTotal { get { return (_mercanciasNumeroTotal ?? "0"); } set { _mercanciasNumeroTotal = value; } }

        #endregion mercancias

        #region mercancia
        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaBienesTransportados { get { return (_mercanciaBienesTransportados ?? ""); } set { _mercanciaBienesTransportados = value; } }

        [StringLength(256, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaDescripcion { get { return (_mercanciaDescripcion ?? ""); } set { _mercanciaDescripcion = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaCantidad { get { return (_mercanciaCantidad ?? ""); } set { _mercanciaCantidad = value; } }

        [StringLength(32, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaClaveUnidad { get { return (_mercanciaClaveUnidad ?? ""); } set { _mercanciaClaveUnidad = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaUnidad { get { return (_mercanciaUnidad ?? ""); } set { _mercanciaUnidad = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaDimensiones { get { return (_mercanciaDimensiones ?? ""); } set { _mercanciaDimensiones = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaMaterialPeligroso { get { return (_mercanciaMaterialPeligroso ?? ""); } set { _mercanciaMaterialPeligroso = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaClaveMarterialPeligroso { get { return (_mercanciaClaveMarterialPeligroso ?? ""); } set { _mercanciaClaveMarterialPeligroso = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaEmbalaje { get { return (_mercanciaEmbalaje ?? ""); } set { _mercanciaEmbalaje = value; } }

        [StringLength(256, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaDescripcionEmbalaje { get { return (_mercanciaDescripcionEmbalaje ?? ""); } set { _mercanciaDescripcionEmbalaje = value; } }

        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaPesoKilogramo { get { return (_mercanciaPesoKilogramo ?? ""); } set { _mercanciaPesoKilogramo = value; } }

        [StringLength(32, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaValor { get { return (_mercanciaValor ?? ""); } set { _mercanciaValor = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaMoneda { get { return (_mercanciaMoneda ?? ""); } set { _mercanciaMoneda = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaFraccionArancelaria { get { return (_mercanciaFraccionArancelaria ?? ""); } set { _mercanciaFraccionArancelaria = value; } }

        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaPedimiento { get { return (_mercanciaPedimiento ?? ""); } set { _mercanciaPedimiento = value; } }

        [StringLength(29, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string MercanciaUUIDComprobanteComercioExterior { get { return (_mercanciaUUIDComprobanteComercioExterior ?? ""); } set { _mercanciaUUIDComprobanteComercioExterior = value; } }

        #endregion mercancia

        #region cantidadtransportada
        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string CantidadTransportada { get { return (_cantidadTransportada ?? ""); } set { _cantidadTransportada = value; } }

        [StringLength(512, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string CantidadTransportadaOrigenID { get { return (_cantidadTransportadaOrigenID ?? ""); } set { _cantidadTransportadaOrigenID = value; } }

        [StringLength(512, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string CantidadTransportadaDestinoID { get { return (_cantidadTransportadaDestinoID ?? ""); } set { _cantidadTransportadaDestinoID = value; } }

        #endregion cantidadtransportada

        #region federal

        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string FederalPermisoSCT { get { return (_federalPermisoSCT ?? ""); } set { _federalPermisoSCT = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string FederalNumeroPermisoSCT { get { return (_federalNumeroPermisoSCT ?? ""); } set { _federalNumeroPermisoSCT = value; } }

        #endregion federal

        #region idvehicular

        [StringLength(8, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string ClaveTipoUnidad { get { return (_claveTipoUnidad ?? ""); } set { _claveTipoUnidad = value; } }
        
        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string TipoUnidad { get { return (_tipoUnidad ?? ""); } set { _tipoUnidad = value; } }

        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string IdVehicularConfiguracion { get { return (_idVehicularConfiguracion ?? ""); } set { _idVehicularConfiguracion = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string IdVehicularPlacaVehiculoMotor { get { return (_idVehicularPlacaVehiculoMotor ?? ""); } set { _idVehicularPlacaVehiculoMotor = value; } }

        [StringLength(32, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string IdVehicularAnioModeloVM { get { return (_idVehicularAnioModeloVM ?? ""); } set { _idVehicularAnioModeloVM = value; } }
      
        #endregion idvehicular

        #region seguros

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string FederalNombreAseguradora { get { return (_federalNombreAseguradora ?? ""); } set { _federalNombreAseguradora = value; } }

        [StringLength(256, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string FederalNumeroPolizaSeguro { get { return (_federalNumeroPolizaSeguro ?? ""); } set { _federalNumeroPolizaSeguro = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string SeguroFederalAseguraMedAmbiente { get { return (_seguroFederalAseguraMedAmbiente ?? ""); } set { _seguroFederalAseguraMedAmbiente = value; } }

        [StringLength(256, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string SeguroFederalNumeroPolizaAseguraMedAmbiente { get { return (_seguroFederalNumeroPolizaAseguraMedAmbiente ?? ""); } set { _seguroFederalNumeroPolizaAseguraMedAmbiente = value; } }

        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string SeguroAseguraCarga { get { return (_seguroAseguraCarga ?? ""); } set { _seguroAseguraCarga = value; } }

        [StringLength(64, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string SeguroPrimaSeguro { get { return (_seguroPrimaSeguro ?? ""); } set { _seguroPrimaSeguro = value; } }

        #endregion seguros

        #region remolque

        [StringLength(8, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string RemolqueSubtipo { get { return (_remolqueSubtipo ?? ""); } set { _remolqueSubtipo = value; } }

        [StringLength(32, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string RemolquePlaca { get { return (_remolquePlaca ?? ""); } set { _remolquePlaca = value; } }

        #endregion remolque

        #region figuratransporte

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string TipoFigura { get { return (_tipoFigura ?? ""); } set { _tipoFigura = value; } }

        [StringLength(16, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OperadorRFC { get { return (_operadorRFC ?? ""); } set { _operadorRFC = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OperadorNumLicencia { get { return (_operadorNumLicencia ?? ""); } set { _operadorNumLicencia = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OperadorNombre { get { return (_operadorNombre ?? ""); } set { _operadorNombre = value; } }

        [StringLength(128, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OperadorRegistroFiscal { get { return (_operadorRegistroFiscal ?? ""); } set { _operadorRegistroFiscal = value; } }

        [StringLength(254, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string OperadorResidenciaFiscal { get { return (_operadorResidenciaFiscal ?? ""); } set { _operadorResidenciaFiscal = value; } }

        [StringLength(256, ErrorMessage = "La longitud máxima para {0} es de {1} caracteres")]
        public string ParteTransporte { get { return (_parteTransporte ?? ""); } set { _parteTransporte = value; } }

        #endregion figuratransporte
    }
}
