using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace anses.DNRPA.Entidad
{
    public class DNRPA_Dominio : IDisposable
    {
        #region Dispose
        private bool disposing;

        public void Dispose()
        {
            // Llamo al método que contiene la lógica
            // para liberar los recursos de esta clase.
            Dispose(true);
        }

        protected virtual void Dispose(bool b)
        {
            // Si no se esta destruyendo ya…
            if (!disposing)
            {
                // La marco como desechada ó desechandose,
                // de forma que no se puede ejecutar este código
                // dos veces.
                disposing = true;

                // Indico al GC que no llame al destructor
                // de esta clase al recolectarla.
                GC.SuppressFinalize(this);
                // … libero los recursos… 
            }
        }
        ~DNRPA_Dominio()
        {
            // Llamo al método que contiene la lógica
            // para liberar los recursos de esta clase.
            Dispose(true);
        }
        #endregion

        public string anioModelo { get; set; }
        public string codigoDominio { get; set; }
        public string codigoProcedencia { get; set; }
        public string codigoVehiculo { get; set; }
        public DateTime fechaTitularidad { get; set; }
        public string marcaCodigo { get; set; }
        public string marcaDescripcion { get; set; }
        public string modeloCodigo { get; set; }
        public string modeloDescripcion { get; set; }
        public string porcentajeTitularidad { get; set; }
        public string tipoCodigo { get; set; }

        public string tipoDescripcion { get; set; }
        public string valuacion { get; set; }

        

        public DNRPA_Dominio() { }

        public DNRPA_Dominio(string _anioModelo,
        string _codigoDominio,
        string _codigoProcedencia,
        string _codigoVehiculo,
         DateTime _fechaTitularidad ,
         string _marcaCodigo ,
         string _marcaDescripcion ,
         string _modeloCodigo ,
         string _modeloDescripcion ,
         string _porcentajeTitularidad ,
         string _tipoCodigo ,
         string _tipoDescripcion ,
         string _valuacion )
        {
            this.anioModelo = _anioModelo;
            this.codigoDominio = _codigoDominio;
            this.codigoProcedencia = _codigoProcedencia;
            this.codigoVehiculo = _codigoVehiculo;
            this.fechaTitularidad = _fechaTitularidad;
            this.marcaCodigo = _marcaCodigo;
            this.marcaDescripcion = _marcaDescripcion;
            this.modeloCodigo = _modeloCodigo;
            this.modeloDescripcion = _modeloDescripcion;
            this.porcentajeTitularidad = _porcentajeTitularidad;
            this.tipoCodigo = _tipoCodigo;
            this.tipoDescripcion = _tipoDescripcion;
            this.valuacion = _valuacion;
        }
    }


    public class DNRPA_Respuesta : IDisposable
    {
        #region Dispose
        private bool disposing;

        public void Dispose()
        {
            // Llamo al método que contiene la lógica
            // para liberar los recursos de esta clase.
            Dispose(true);
        }

        protected virtual void Dispose(bool b)
        {
            // Si no se esta destruyendo ya…
            if (!disposing)
            {
                // La marco como desechada ó desechandose,
                // de forma que no se puede ejecutar este código
                // dos veces.
                disposing = true;

                // Indico al GC que no llame al destructor
                // de esta clase al recolectarla.
                GC.SuppressFinalize(this);
                // … libero los recursos… 
            }
        }
        ~DNRPA_Respuesta()
        {
            // Llamo al método que contiene la lógica
            // para liberar los recursos de esta clase.
            Dispose(true);
        }
        #endregion

        public List<DNRPA_Dominio> listaDominios{ get; set; }
        public string mensajeError { get; set; }
        
        public DNRPA_Respuesta() { }

        public DNRPA_Respuesta(List<DNRPA_Dominio> _listaDominios,
            string _ErrorMensaje)
        {
            this.listaDominios = _listaDominios;
            this.mensajeError = _ErrorMensaje;
        }
    }
}
