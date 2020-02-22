using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Services;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using log4net;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web.UI;
using anses.DNRPA.Entidad;



/// <summary>
/// Descripción breve de servicioDNRPA
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
// [System.Web.Script.Services.ScriptService]
public class servicioDNRPA : System.Web.Services.WebService
{
    private static readonly ILog log = LogManager.GetLogger(typeof(servicioDNRPA).Name);

    public servicioDNRPA()
    {
    }

    #region VALIDACIONES

    #region Valida Ingreso de Numeros

    private static bool esNumerico(string Valor)
    {
        try
        {
            long.Parse(Valor);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool ValidaCUIL(string CUIL)
    {

        string patron = @"^\d{11}$";
        Regex re = new Regex(patron);

        bool resp = re.IsMatch(CUIL);

        if (resp)
        {

            string FACTORES = "54327654321";
            double dblSuma = 0;

            if (!(CUIL.Substring(0, 1).ToString() != "3" && CUIL.Substring(0, 1).ToString() != "2"))
            {
                for (int i = 0; i < 11; i++)
                    dblSuma = dblSuma + int.Parse(CUIL.Substring(i, 1).ToString()) * int.Parse(FACTORES.Substring(i, 1).ToString());
            }
            resp = Math.IEEERemainder(dblSuma, 11) == 0;
        }
        return resp;
    }

    #endregion

    #endregion

    //Este método obtiene el certificado de una URL - no es necesario registrar o instalar el certificado en el servidor
    [WebMethod(Description = "Obtiene una lista de dominios 'Vehículos' registrados a un CUIL o CUIT ")]
    public DNRPA_Respuesta Dominios_Traer(string CUIL_CUIT)
    {
        DNRPA_Respuesta Respuesta = null;
        
        try
        {
            if (log.IsInfoEnabled)
                log.Info("Ingreso al método " + MethodBase.GetCurrentMethod().Name);

            if (CUIL_CUIT.Length != 11 || !esNumerico(CUIL_CUIT) || !ValidaCUIL(CUIL_CUIT))
            {
                //throw new Exception("El CUIL consultado no es válido");
                Respuesta = new DNRPA_Respuesta(null, "El CUIL consultado no es válido");
                return Respuesta;
            }

            rpa.WSDominiosPorCuitANSES serv = new rpa.WSDominiosPorCuitANSES();
            serv.Url = ConfigurationManager.AppSettings[serv.GetType().ToString()];
            serv.Credentials = System.Net.CredentialCache.DefaultCredentials;

            if (log.IsDebugEnabled)
                log.Debug("Recupero el proxy del servidor para asignárselo al servicio");

            WebProxy proxyObject = new WebProxy(ConfigurationManager.AppSettings["ProxyName"], int.Parse(ConfigurationManager.AppSettings["ProxyPort"]));
            proxyObject.BypassProxyOnLocal = true;



            if (proxyObject.Address != null)
            {
                proxyObject.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                serv.Proxy = new System.Net.WebProxy(proxyObject.Address, proxyObject.BypassProxyOnLocal, proxyObject.BypassList, proxyObject.Credentials);

                if (log.IsDebugEnabled)
                    log.Debug("el proxy es " + proxyObject.Address);

            }
            else
            {
                //throw new Exception("No se pudo obtener el proxy del servidor");
                Respuesta = new DNRPA_Respuesta(null, "No se pudo obtener el proxy del servidor");
                return Respuesta;
            }

            if (log.IsDebugEnabled)
                log.Debug("Recupero el certificado de DNRPA");

            System.Security.Cryptography.X509Certificates.X509Certificate2 certLocal = new X509Certificate2();
            string Certificado = ConfigurationManager.AppSettings["Path_Certificado"];

            if (!File.Exists(Certificado))
            {
                log.Error("No se pudo obtener el certificado en: " + Certificado);
                //throw new ApplicationException("No se pudo obtener el archivo del certificado dnrpa");
                Respuesta = new DNRPA_Respuesta(null, "No se pudo obtener el archivo del certificado dnrpa");
                return Respuesta;
            }
            else
            {

                log.Debug("URL del Certificado es: " + Certificado);

                //configura el certificado local - no del almacen

                // X509KeyStorageFlags.PersistKeySet y X509KeyStorageFlags.MachineKeySet - Crea un archivo en C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys, 
                //servidores tiro la bronca porque no se puden eliminar los archivos, al quitar la entrada deja de registrar la ejecucion
                certLocal.Import(Certificado, ConfigurationManager.AppSettings["Clave_Certificado"], X509KeyStorageFlags.DefaultKeySet);
                
                List<System.Security.Cryptography.X509Certificates.X509Certificate2> iLCert = new List<X509Certificate2>();

                //primero se ingresa el certificado local
                iLCert.Add(certLocal);
                log.InfoFormat("El certificado emitido por: " + certLocal.Issuer + " expira: " + certLocal.GetExpirationDateString());


                List<rpa.dominioType> g = new List<rpa.dominioType>();

                if (iLCert.Count == 0)
                    log.Debug("no se obtuvo certificado o no existe instalado");
                else
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)

                    { return true; };


                    
                    serv.ClientCertificates.AddRange(iLCert.ToArray());

                    log.Debug("Tengo " + serv.ClientCertificates.Count + " certificados");

                    if (log.IsDebugEnabled)
                        log.DebugFormat("Ejecuto el servicio DNRPA con el CUIL {0}", CUIL_CUIT);

                    var tiempo = Stopwatch.StartNew();
                    g = serv.consultar(CUIL_CUIT).ToList();
                    tiempo.Stop();


                    if (log.IsInfoEnabled)
                        log.InfoFormat("el servicio {0} tardo {1} ", MethodBase.GetCurrentMethod().Name, tiempo.Elapsed);

                    if (log.IsDebugEnabled)
                        log.DebugFormat("se obtuvieron {0} regitros", g.Count);
                }

                if (g != null)
                {
                    Respuesta = new DNRPA_Respuesta(TDominios(g) , string.Empty );
                }
                return Respuesta;
            }
        }
        catch (CryptographicException ex)
        {
            log.Error(string.Format("{0} - Error Cryptographic:{1}->{2}-->{3}-->{4}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Source, ex.Message, ex.StackTrace, ex.Data));
            Respuesta = new DNRPA_Respuesta(null, ex.Message);
            return Respuesta;

            //throw ex;
        }
        catch (Exception err)
        {
            log.Error(string.Format("{0} - Error:{1}->{2}-->{3}-->{4}", System.Reflection.MethodBase.GetCurrentMethod().Name, err.Source, err.Message, err.StackTrace, err.Data));
            Respuesta = new DNRPA_Respuesta(null, err.Message);
            return Respuesta;
            //throw err;
        }
        finally
        {
            log.Info("Salgo del método " + MethodBase.GetCurrentMethod().Name);
        }
    }

    private List<DNRPA_Dominio> TDominios(List<rpa.dominioType> lDom)
    {
        List<DNRPA_Dominio> listaSalida = new List<DNRPA_Dominio>();
        foreach (rpa.dominioType dom in lDom)
        {
            listaSalida.Add( new DNRPA_Dominio(dom.anioModelo, dom.codigoDominio, dom.codigoProcedencia, dom.codigoVehiculo, DateTime.Parse(dom.fechaTitularidad), dom.marcaCodigo, dom.marcaDescripcion
                , dom.modeloCodigo, dom.modeloDescripcion, dom.porcentajeTitularidad, dom.tipoCodigo, dom.tipoDescripcion, dom.valuacion));
        }
        return listaSalida;
    }
}
