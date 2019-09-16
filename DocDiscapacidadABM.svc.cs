using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI.HtmlControls;
using System.Configuration;

namespace WcfServiceAPE
{
    [ServiceBehavior(Namespace = "http://ar.com.osde/osgapeservice/", Name = "OSGAPEBackendService")]
    public class WS4 : DocDiscapacidadABM
    {
        string DocDiscapacidadABM.cargarCertificado(string xFilialLogin, string xNroafil, string xTransporte, string xFvto, string xPatologia, string xFecert, string xIDcabecera, string xIDDOC, string xUsuario, string xIDPapel, string xAccion, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string xfilAfil = xNroafil.ToString().Substring(0, 2);
            string xnroAfil = xNroafil.ToString().Substring(3, 7);
            string xBeneAfil = xNroafil.ToString().Substring(11, 2);

            string Abm_Doc_Certif = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                string idPto = UltimoNroIdPto(xFilialLogin, xModulo, xBase, xUserLog,xFilialLog);

                sSql = "";
                sSql = sSql + "INSERT INTO GRAL_CERTIFICADOS  ";
                sSql = sSql + " VALUES ('" + idPto + "',";
                sSql = sSql + "" + xfilAfil + ",";
                sSql = sSql + "" + xnroAfil + ",";
                sSql = sSql + "" + xBeneAfil + ",";
                sSql = sSql + "'" + xTransporte + "',";
                sSql = sSql + "'" + FormatoFecha(xFvto, "yyyy/mm/dd", true) + "',";
                sSql = sSql + "'" + xPatologia + "',";
                sSql = sSql + "" + "GetDate()" + ",";
                sSql = sSql + "'" + xUsuario + "',";
                sSql = sSql + "'" + FormatoFecha(xFecert, "yyyy/mm/dd", true) + "',";
                sSql = sSql + "'" + "NO" + "',";
                sSql = sSql + "'" + "NO" + "')";

                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("abmdoccertif");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    //-------------------------------------

                    Abm_Doc_Certif = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                    //}
                    // Commiteo uno o los dos INSERTs
                    transaction.Commit();

                    //Invoco al metodo abmdocdisca 
                    String a = this.abmDocDisca(xIDcabecera, xIDDOC, xUsuario, idPto, xAccion, xModulo,  xBase,  xUserLog,  xFilialLog);
                    xMensaje = a;
                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("abmdoccertif");
                    Abm_Doc_Certif = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return Abm_Doc_Certif;
            }
        }
        string DocDiscapacidadABM.validarCertificado(string xAfiliado, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xfilAfil;
            string xnroAfil;
            string xBeneAfil;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string Consulta_Certificado = "";

            string valCert = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (xAfiliado == null)
            {
                valCert = "[]";
                return valCert;
            }
            //61-8076475-01
            //0123456789012
            xfilAfil = xAfiliado.ToString().Substring(0, 2);
            xnroAfil = xAfiliado.ToString().Substring(3, 7);
            xBeneAfil = xAfiliado.ToString().Substring(11, 2);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT isnull(MAX(IDCERT),0) AS MAXIMO, COUNT(IDCERT) AS CANTIDAD ";
                sSql = sSql + "FROM GRAL_CERTIFICADOS  WITH(NOLOCK)   ";
                sSql = sSql + " WHERE  FILAFIL = " + xfilAfil + " AND NROAFIL = " + xnroAfil + " AND BENEFAFIL = " + xBeneAfil + " AND FEVTO >= getdate()";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);


                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            valCert = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (int.Parse(dt.Rows[i]["Cantidad"].ToString()) > 1)
                                {
                                    xMensaje = "Tiene más de 1, ver que se hace";
                                    Consulta_Certificado = "0";
                                }
                                else
                                {
                                    if (int.Parse(dt.Rows[i]["Cantidad"].ToString()) == 1)
                                    {
                                        Consulta_Certificado = dt.Rows[i]["MAXIMO"].ToString();
                                    }
                                    else
                                    {
                                        Consulta_Certificado = "0";
                                    }
                                }


                                valCert = valCert + "{";
                                valCert = valCert + '"' + "certificado" + '"' + ":" + '"' + Consulta_Certificado + '"' + ",";
                                valCert = valCert + '"' + "mensaje" + '"' + ":" + '"' + xMensaje + '"';

                                if (i < Contador - 1)
                                {
                                    valCert = valCert + "},";
                                }
                                else
                                {
                                    valCert = valCert + "}";
                                };
                            }
                            valCert = valCert + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            valCert = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        valCert = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            valCert = valCert + "{";
                            valCert = valCert + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                valCert = valCert + "}";
                            }
                            else
                            {
                                valCert = valCert + "},";
                            };
                        }
                        valCert = valCert + "]";
                    }
                    con.Close();
                    return valCert;

                }
            }

        }

        public string UltimoNroIdPto(string sFilial, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string idPto;
            float idPtof;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "";
                sSQL = sSQL + "SELECT isnull(MAX(IDCERT),0) AS MAXIMO FROM GRAL_CERTIFICADOS WITH(NOLOCK) ";
                sSQL = sSQL + " WHERE  IDCERT >=" + sFilial + "00000001 AND IDCERT <=" + sFilial + "99999999";

                using (SqlCommand cmd = new SqlCommand(sSQL, con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        if (int.Parse(dt.Rows[0]["MAXIMO"].ToString()) > 0)
                        {
                            idPto = (dt.Rows[0]["MAXIMO"].ToString());
                        }
                        else
                        { idPto = sFilial + "00000000"; }

                        idPtof = float.Parse(idPto) + 1;
                        idPto = idPtof.ToString();

                    }
                    catch (Exception ex)
                    {
                        idPto = sFilial + "00000000";
                        con.Close();
                    }
                    return idPto;
                }
            }
        }

        public bool VerifHabilAsignada(string xIdHab, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            bool idPto;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "";
                sSQL = sSQL + "Select isnull(count(*),0) as CANTIDAD FROM CAB_DOCUMENTACION_DISCA ";
                sSQL = sSQL + " WHERE IDCAB = [IDCAB] AND IDDOC = 3 and Idpapel = " + xIdHab;

                using (SqlCommand cmd = new SqlCommand(sSQL, con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        if (Double.Parse(dt.Rows[0]["CANTIDAD"].ToString()) > 0)
                        {
                            idPto = true;
                        }
                        else
                        { idPto = false; }
                    }
                    catch (Exception ex)
                    {
                        idPto = false;
                        con.Close();
                    }
                    return idPto;
                }
            }
        }
        public string UltimoNroIdHabilitaciones(string sFilial, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string idPto;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "";
                sSQL = sSQL + "SELECT isnull(MAX(IDHABIL ),0)+1 AS MAXIMO FROM GRAL_HABILITACIONES WITH(NOLOCK) ";
                sSQL = sSQL + " WHERE  IDHABIL  >=" + sFilial + "00000001 AND IDHABIL  <=" + sFilial + "99999999";

                using (SqlCommand cmd = new SqlCommand(sSQL, con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        if (Double.Parse(dt.Rows[0]["MAXIMO"].ToString()) > 0)
                        {
                            idPto = (dt.Rows[0]["MAXIMO"].ToString());
                        }
                        else
                        { idPto = sFilial + "00000000"; }
                    }
                    catch (Exception ex)
                    {
                        idPto = sFilial + "00000000";
                        con.Close();
                    }
                    return idPto;
                }
            }
        }

        private string FormatoFecha(string xDate, String xFormat, bool xTime)
        {
            string xDia = "";
            string xMes = "";
            string xAnio = "";
            int xLugares = 0;
            int i;
            bool xEsNumero;

            if (xDate == null || xDate == "" || xDate == "01/01/1900 00:00:00" || xDate == "01-01-1900 00:00:00" || xDate == "1900-01-01 00:00:00.000")
            {
                //Fecha en blanco o null, retorno como viene...
                return xDate;
            }

            string MyDate = xDate;
            DateTime date = Convert.ToDateTime(MyDate);

            int Year = date.Year;
            int Month = date.Month;
            int Day = date.Day;

            xAnio = Year.ToString();
            xMes = Month.ToString();
            xDia = Day.ToString();

            if (Day < 10)
            {
                xDia = "0" + xDia;
            }

            if (Month < 10)
            {
                xMes = "0" + xMes;
            }

            if (Year < 10)
            {
                xAnio = "20" + xAnio;
            }


            switch (xFormat)
            {
                case "mm/dd/yyyy":
                    xDate = xMes + "/" + xDia + "/" + xAnio;
                    break;
                case "dd/mm/yyyy":
                    xDate = xDia + "/" + xMes + "/" + xAnio;
                    break;
                case "yyyy/mm/dd":
                    xDate = xAnio + "/" + xMes + "/" + xDia;
                    break;
                //default:
                //    Console.WriteLine("Default case");
                //    break;            }
            }
            if (xTime == true)
            {
                //Fecha en blanco o null, retorno como viene...
                xDate = xDate + " 00:00:00";
            }
            return xDate;
        }


        public string abmDocDisca(string xIDcabecera, string xIDDOC, string xUser, string xIDPapel, string xAccion, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();

            string Abm_Doc_Disca = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {


                if (xAccion == "A")
                {
                    xMensaje = "OK";
                    if (xIDDOC == "7")
                    {
                        sSql = "";
                        sSql = sSql + "INSERT INTO CAB_DOCUMENTACION_DISCA ";
                        sSql = sSql + " VALUES ('" + xIDcabecera + "',";
                        sSql = sSql + "'" + xIDDOC + "',";
                        sSql = sSql + "" + "GetDate()" + ",";
                        sSql = sSql + "'" + xUser + "',";
                        sSql = sSql + "'" + xIDPapel + "',";
                        sSql = sSql + "'" + "NO" + "',";
                        sSql = sSql + "'" + "NO" + "')";
                    }
                    else
                    {
                        sSql = "";
                        sSql = sSql + "INSERT INTO CAB_DOCUMENTACION_DISCA ";
                        sSql = sSql + " VALUES ('" + xIDcabecera + "',";
                        sSql = sSql + "'" + xIDDOC + "',";
                        sSql = sSql + "" + "GetDate()" + ",";
                        sSql = sSql + "'" + xUser + "',";
                        sSql = sSql + "" + "NULL" + ",";
                        sSql = sSql + "'" + "NO" + "',";
                        sSql = sSql + "'" + "NO" + "')";

                    }
                }
                else
                {
                    xMensaje = "El Documento fue Borrado";
                    sSql = "DELETE FROM CAB_DOCUMENTACION_DISCA WHERE IDCAB =" + xIDcabecera + " AND IDDOC =" + xIDDOC;
                }

                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("abmdocdisca");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    //-------------------------------------

                    Abm_Doc_Disca = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    xMensaje = "NO OK";
                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("abmdocdisca");
                    Abm_Doc_Disca = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return Abm_Doc_Disca;
            }
        }
        public string prest_relacionado(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xCUIT, xNOMBREPRESTADOR;

            string prestRela = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (xIDCabecera == null)
            {
                prestRela = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "ID de Afiliado esta vacio" + '"' + "}]";
                return prestRela;
            }
            //61-8076475-01
            //0123456789012


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT max(reng.id) as maximo, reng.CODPRESTADOR,max(prest.NOMPRESTADOR) as NombrePrestador,max (CASE WHEN prest.APELLIDO IS NOT NULL AND prest.NOMBRE IS NOT NULL THEN prest.NOMBRE + ' ' + prest.APELLIDO ";
                sSql = sSql + " WHEN prest.APELLIDO IS NULL AND prest.NOMBRE IS NOT NULL THEN prest.NOMBRE ELSE prest.APELLIDO END) AS NOMPRESTADOR2,max(CUIT) as CUIT";
                sSql = sSql + " FROM RENGLONES as reng";
                sSql = sSql + " left OUTER Join GRAL_PRESTADORES as prest on reng.CODPRESTADOR = prest.CODPRESTADOR";
                sSql = sSql + " WHERE IDCAB = " + xIDCabecera;
                sSql = sSql + " GROUP BY reng.CODPRESTADOR";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);


                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            prestRela = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                if (dt.Rows[i]["CUIT"].ToString() == null)
                                {
                                    xCUIT = "No Existe";
                                }
                                else
                                {
                                    xCUIT = dt.Rows[i]["CUIT"].ToString();
                                }

                                if (dt.Rows[i]["NOMBREPRESTADOR"].ToString() == null)
                                {
                                    xNOMBREPRESTADOR = "No Existe";
                                }
                                else
                                {
                                    xNOMBREPRESTADOR = dt.Rows[i]["NOMBREPRESTADOR"].ToString();
                                }


                                prestRela = prestRela + "{";
                                prestRela = prestRela + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["maximo"].ToString() + '"' + ",";
                                prestRela = prestRela + '"' + "CUIT" + '"' + ":" + '"' + xCUIT + '"' + ",";
                                prestRela = prestRela + '"' + "Prestador" + '"' + ":" + '"' + xNOMBREPRESTADOR + '"';

                                if (i < Contador - 1)
                                {
                                    prestRela = prestRela + "},";
                                }
                                else
                                {
                                    prestRela = prestRela + "}";
                                };
                            }
                            prestRela = prestRela + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            prestRela = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        prestRela = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            prestRela = prestRela + "{";
                            prestRela = prestRela + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                prestRela = prestRela + "}";
                            }
                            else
                            {
                                prestRela = prestRela + "},";
                            };
                        }
                        prestRela = prestRela + "]";
                    }
                    con.Close();
                    return prestRela;

                }
            }

        }
        public string cargar_habilitaciones(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xCUIT, xNOMBREPRESTADOR, xINSCRNP;

            string cargHabi = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (xIDCabecera == null)
            {
                cargHabi = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "ID de Cabecera esta vacio" + '"' + "}]";
                return cargHabi;
            }
            //61-8076475-01
            //0123456789012


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT  disc.IDPAPEL, hab.CUIT,pres.NOMPRESTADOR, hab.INSCRNP, (CASE WHEN pres.APELLIDO IS NOT NULL AND pres.NOMBRE IS NOT NULL THEN pres.NOMBRE + ' ' + pres.APELLIDO ";
                sSql = sSql + " WHEN pres.APELLIDO IS NULL AND pres.NOMBRE IS NOT NULL THEN pres.NOMBRE ELSE pres.APELLIDO END) AS NOMPRESTADOR2";
                sSql = sSql + " FROM CAB_DOCUMENTACION_DISCA as disc  WITH(NOLOCK) ";
                sSql = sSql + " left OUTER JOIN GRAL_HABILITACIONES as hab on hab.IDHABIL  = disc.IDPAPEL";
                sSql = sSql + " left OUTER JOIN GRAL_PRESTADORES as pres on hab.CUIT = pres.CODPRESTADOR";
                sSql = sSql + " WHERE IDCAB =" + xIDCabecera + " AND IDDOC = 3";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);


                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            cargHabi = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                if (dt.Rows[i]["INSCRNP"].ToString() == null)
                                {
                                    xINSCRNP = "No Existe";
                                }
                                else
                                {
                                    xINSCRNP = dt.Rows[i]["INSCRNP"].ToString();
                                }

                                if (dt.Rows[i]["CUIT"].ToString() == null)
                                {
                                    xCUIT = "No Existe";
                                }
                                else
                                {
                                    xCUIT = dt.Rows[i]["CUIT"].ToString();
                                }

                                if (dt.Rows[i]["NOMPRESTADOR"].ToString() == null)
                                {
                                    xNOMBREPRESTADOR = "No Existe";
                                }
                                else
                                {
                                    xNOMBREPRESTADOR = dt.Rows[i]["NOMPRESTADOR"].ToString();
                                }


                                cargHabi = cargHabi + "{";
                                cargHabi = cargHabi + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["IDPAPEL"].ToString() + '"' + ",";
                                cargHabi = cargHabi + '"' + "CUIT" + '"' + ":" + '"' + xCUIT + '"' + ",";
                                cargHabi = cargHabi + '"' + "Prestador" + '"' + ":" + '"' + xNOMBREPRESTADOR + '"' + ",";
                                cargHabi = cargHabi + '"' + "RNOS" + '"' + ":" + '"' + xINSCRNP + '"';

                                if (i < Contador - 1)
                                {
                                    cargHabi = cargHabi + "},";
                                }
                                else
                                {
                                    cargHabi = cargHabi + "}";
                                };
                            }
                            cargHabi = cargHabi + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cargHabi = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cargHabi = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cargHabi = cargHabi + "{";
                            cargHabi = cargHabi + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cargHabi = cargHabi + "}";
                            }
                            else
                            {
                                cargHabi = cargHabi + "},";
                            };
                        }
                        cargHabi = cargHabi + "]";
                    }
                    con.Close();
                    return cargHabi;

                }
            }

        }
        public string cargar_prestador(string xCUIT, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string cargPres = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (xCUIT == null)
            {
                cargPres = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "El CUIT esta vacio" + '"' + "}]";
                return cargPres;
            }
            //61-8076475-01
            //0123456789012


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT pres.NOMPRESTADOR, (CASE WHEN pres.APELLIDO IS NOT NULL AND pres.NOMBRE IS NOT NULL ";
                sSql = sSql + " THEN pres.NOMBRE + ' ' + pres.APELLIDO WHEN pres.APELLIDO IS NULL AND pres.NOMBRE IS NOT NULL ";
                sSql = sSql + " THEN pres.NOMBRE ELSE pres.APELLIDO END) AS NOMPRESTADOR2, hab.IDHABIL, hab.INSCRNP, hab.MODALIDAD, hab.CATEG, hab.FEVTO, hab.NROHAB";
                sSql = sSql + " FROM GRAL_PRESTADORES as pres  WITH(NOLOCK) ";
                sSql = sSql + " Left OUTER JOIN GRAL_HABILITACIONES as hab ON hab.CUIT = pres.CODPRESTADOR";
                sSql = sSql + " WHERE CODPRESTADOR ='" + xCUIT + "'";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            cargPres = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                cargPres = cargPres + "{";
                                cargPres = cargPres + '"' + "Prestador" + '"' + ":" + '"' + dt.Rows[i]["NOMPRESTADOR"].ToString() + '"' + ",";
                                cargPres = cargPres + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["IDHABIL"].ToString() + '"' + ",";
                                cargPres = cargPres + '"' + "INSCRNP" + '"' + ":" + '"' + dt.Rows[i]["INSCRNP"].ToString() + '"' + ",";
                                cargPres = cargPres + '"' + "modalidad" + '"' + ":" + '"' + dt.Rows[i]["MODALIDAD"].ToString() + '"' + ",";
                                cargPres = cargPres + '"' + "Categoria" + '"' + ":" + '"' + dt.Rows[i]["CATEG"].ToString() + '"' + ",";
                                cargPres = cargPres + '"' + "Fevto" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FEVTO"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                cargPres = cargPres + '"' + "Habilitacion" + '"' + ":" + '"' + dt.Rows[i]["NROHAB"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    cargPres = cargPres + "},";
                                }
                                else
                                {
                                    cargPres = cargPres + "}";
                                };
                            }
                            cargPres = cargPres + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cargPres = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "El prestador ingresado no existe." + '"' + "}]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cargPres = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cargPres = cargPres + "{";
                            cargPres = cargPres + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cargPres = cargPres + "}";
                            }
                            else
                            {
                                cargPres = cargPres + "},";
                            };
                        }
                        cargPres = cargPres + "]";
                    }
                    con.Close();
                    return cargPres;

                }
            }

        }
        public string nueva_habilitacion(string xFilial, string xCUIT, string xINSCRNP, string xModalidad, string xCATEG, string xHAB, string xFEVTO, string xUSUARIO, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string nuevaHab = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (xCUIT == null || xModalidad == null || xFilial == null || xFEVTO == null)
            {
                nuevaHab = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "El CUIT o la Modalidad o la Filial o la FEcha de Vencimiento no pueden estar vacios." + '"' + "}]";
                return nuevaHab;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                string xidHab = UltimoNroIdHabilitaciones(xFilial,  xModulo,  xBase,  xUserLog,  xFilialLog);

                sSql = "";
                sSql = sSql + "INSERT INTO GRAL_HABILITACIONES  ";
                sSql = sSql + " VALUES ('" + xCUIT + "',";
                sSql = sSql + "" + xINSCRNP + ",";
                sSql = sSql + "'" + xModalidad + "',";
                sSql = sSql + "'" + xCATEG + "',";
                sSql = sSql + "'" + xHAB + "',";
                sSql = sSql + "'" + FormatoFecha(xFEVTO, "yyyy/mm/dd", true) + "',";
                sSql = sSql + "" + "GetDate()" + ",";
                sSql = sSql + "'" + xUSUARIO + "',";
                sSql = sSql + "'" + xidHab + "',";
                sSql = sSql + "'" + "NO" + "',";
                sSql = sSql + "'" + "NO" + "')";

                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("nuevahabilitacion");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    //-------------------------------------

                    //nuevaHab = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                    //}
                    // Commiteo uno o los dos INSERTs
                    transaction.Commit();

                    //Invoco al metodo cargar_prestador 
                    String a = this.cargar_prestador(xCUIT, xModulo,  xBase, xUserLog, xFilialLog);
                    xMensaje = a;

                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("nuevahabilitacion");
                    xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return xMensaje;
            }
        }
        public string asignar_habilitacion(string xIDCABECERA, string xUsuario, string xIdHab, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string nuevaHab = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                bool xHabilAsig = VerifHabilAsignada(xIdHab, xModulo, xBase, xUserLog, xFilialLog);

                if (xHabilAsig == true)
                {
                    xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "La habilitación ya ha sido asignada al afiliado." + '"' + "}]";
                    return xMensaje;
                }
                sSql = "";
                sSql = sSql + "INSERT INTO CAB_DOCUMENTACION_DISCA  ";
                sSql = sSql + " VALUES ('" + xIDCABECERA + "',";
                sSql = sSql + "" + 3 + ",";
                sSql = sSql + "" + "GetDate()" + ",";
                sSql = sSql + "'" + xUsuario + "',";
                sSql = sSql + "'" + xIdHab + "',";
                sSql = sSql + "'" + "NO" + "',";
                sSql = sSql + "'" + "NO" + "')";

                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("nuevahabilitacion");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    //-------------------------------------

                    //nuevaHab = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                    //}
                    // Commiteo uno o los dos INSERTs
                    transaction.Commit();

                    //Invoco al metodo cargar_habilitaciones 
                    String a = this.cargar_habilitaciones(xIDCABECERA, xModulo,  xBase,  xUserLog,  xFilialLog);
                    xMensaje = a;

                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("nuevahabilitacion");
                    xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return xMensaje;
            }
        }
        public string borrar_habilitacion(string xIDpapel, string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string nuevaHab = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                if (xIDpapel == null || xIDCabecera == null)
                {
                    xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Debe seleccionar un registro a borrar." + '"' + "}]";
                    return xMensaje;
                }
                sSql = "";
                sSql = sSql + "DELETE FROM CAB_DOCUMENTACION_DISCA ";
                sSql = sSql + "WHERE IDPAPEL = " + xIDpapel + " AND  IDCAB = " + xIDCabecera + " AND IDDOC = 3";


                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("borrarhabilitacion");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    //-------------------------------------

                    //nuevaHab = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                    //}
                    // Commiteo uno o los dos INSERTs
                    transaction.Commit();

                    //Invoco al metodo cargar_habilitaciones 
                    String a = this.cargar_habilitaciones(xIDCabecera, xModulo, xBase, xUserLog,  xFilialLog);
                    xMensaje = a;

                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("borrarhabilitacion");
                    xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return xMensaje;
            }
        }
        public string cargar_modalidad(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xCUIT, xNOMBREPRESTADOR, xINSCRNP;

            string cargHabi = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            //61-8076475-01
            //0123456789012


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT CODESP AS CODIGO FROM GRAL_ESPECIALIDADDISCA  WITH(NOLOCK) ";
                sSql = sSql + " WHERE ESTESP ='A' UNION ALL SELECT CODMOD AS CODIGO FROM GRAL_MODALIDADESDISCA ";
                sSql = sSql + " WHERE ESTMOD ='A' ORDER BY CODIGO";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);


                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            cargHabi = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {



                                cargHabi = cargHabi + "{";
                                cargHabi = cargHabi + '"' + "Modalidad" + '"' + ":" + '"' + dt.Rows[i]["CODIGO"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    cargHabi = cargHabi + "},";
                                }
                                else
                                {
                                    cargHabi = cargHabi + "}";
                                };
                            }
                            cargHabi = cargHabi + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cargHabi = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cargHabi = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cargHabi = cargHabi + "{";
                            cargHabi = cargHabi + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cargHabi = cargHabi + "}";
                            }
                            else
                            {
                                cargHabi = cargHabi + "},";
                            };
                        }
                        cargHabi = cargHabi + "]";
                    }
                    con.Close();
                    return cargHabi;

                }
            }

        }
        public string cargar_pm(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string cargarPm = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            //61-8076475-01
            //0123456789012


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT PM.id, PM.FEPEDIDO,PM.solicitud, PM.REQCERTAL, Disca.IDPAPEL";
                sSql = sSql + " FROM CAB_DOCUMENTACION_PM as PM ";
                sSql = sSql + " left OUTER JOIN CAB_DOCUMENTACION_DISCA as Disca ON Disca.IDPAPEL = PM.id and PM.IDCAB = Disca.IDCAB AND Disca.IDDOC = 9";
                sSql = sSql + " WHERE PM.IDCAB =" + xIDCabecera;

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);


                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            cargarPm = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                cargarPm = cargarPm + "{";
                                cargarPm = cargarPm + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["id"].ToString() + '"' + ",";
                                cargarPm = cargarPm + '"' + "Fecha" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FEPEDIDO"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                cargarPm = cargarPm + '"' + "PresSolicitada" + '"' + ":" + '"' + dt.Rows[i]["solicitud"].ToString() + '"' + ",";
                                cargarPm = cargarPm + '"' + "ReqCert" + '"' + ":" + '"' + dt.Rows[i]["REQCERTAL"].ToString() + '"' + ",";
                                cargarPm = cargarPm + '"' + "TieneCert" + '"' + ":" + '"' + dt.Rows[i]["IDPAPEL"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    cargarPm = cargarPm + "},";
                                }
                                else
                                {
                                    cargarPm = cargarPm + "}";
                                };
                            }
                            cargarPm = cargarPm + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cargarPm = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cargarPm = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cargarPm = cargarPm + "{";
                            cargarPm = cargarPm + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cargarPm = cargarPm + "}";
                            }
                            else
                            {
                                cargarPm = cargarPm + "},";
                            };
                        }
                        cargarPm = cargarPm + "]";
                    }
                    con.Close();
                    return cargarPm;

                }
            }

        }
        public string cargar_prestacion(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string cargarPre = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            //61-8076475-01
            //0123456789012


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT CODESP AS CODIGO FROM GRAL_ESPECIALIDADDISCA WITH(NOLOCK)WHERE ESTESP ='A' ";
                sSql = sSql + " UNION ALL ";
                sSql = sSql + " SELECT CODMOD AS CODIGO FROM GRAL_MODALIDADESDISCA WITH(NOLOCK) WHERE ESTMOD ='A' ";
                sSql = sSql + " ORDER BY CODIGO";


                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);


                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            cargarPre = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {



                                cargarPre = cargarPre + "{";
                                cargarPre = cargarPre + '"' + "Prestacion" + '"' + ":" + '"' + dt.Rows[i]["CODIGO"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    cargarPre = cargarPre + "},";
                                }
                                else
                                {
                                    cargarPre = cargarPre + "}";
                                };
                            }
                            cargarPre = cargarPre + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cargarPre = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cargarPre = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cargarPre = cargarPre + "{";
                            cargarPre = cargarPre + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cargarPre = cargarPre + "}";
                            }
                            else
                            {
                                cargarPre = cargarPre + "},";
                            };
                        }
                        cargarPre = cargarPre + "]";
                    }
                    con.Close();
                    return cargarPre;

                }
            }

        }
        public string alta_PM(string xFilial, string xfechapm, string xIDcabecera, string xUsuario, string xPrestacion, string xReqcertal, string xTienecert, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            //sFilial, fechapm, ID, user, prest 
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string nuevaHab = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (xFilial == null || xfechapm == null || xIDcabecera == null || xUsuario == null || xPrestacion == null)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Los Nombres de los campos Filial,xfechapm,IDcabecera,Usuario,Prestacion deben completarse" + '"' + "}]";
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                string xIdMAx = UltimoNroIdHabilitaciones(xFilial, xModulo, xBase, xUserLog, xFilialLog);

                sSql = "";
                sSql = sSql + "INSERT INTO CAB_DOCUMENTACION_PM  ";
                sSql = sSql + " VALUES ('" + FormatoFecha(xfechapm, "yyyy/mm/dd", true) + "',";
                sSql = sSql + "'" + xIDcabecera + "',";
                sSql = sSql + "" + "GetDate()" + ",";
                sSql = sSql + "'" + xUsuario + "',";
                sSql = sSql + "'" + xIdMAx + "',";
                sSql = sSql + "'" + xPrestacion + "',";
                sSql = sSql + "'" + xReqcertal + "',";
                sSql = sSql + "'" + "NO" + "',";
                sSql = sSql + "'" + "NO" + "')";

                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("nuevadocumentacionpm");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;

                    //-------------------------------------
                    if (command.ExecuteNonQuery() > 0) // Logro el insert
                    {
                        sSql = " INSERT INTO CAB_DOCUMENTACION_DISCA ";
                        sSql = sSql + " VALUES ";
                        sSql = sSql + "('" + xIDcabecera + "',";
                        sSql = sSql + "" + "4" + ",";
                        sSql = sSql + "" + "GetDate()" + ",";
                        sSql = sSql + "'" + xUsuario + "',";
                        sSql = sSql + "'" + xIdMAx + "',";
                        sSql = sSql + "'" + "NO" + "',";
                        sSql = sSql + "'" + "NO" + "')";

                        command.CommandText = sSql;
                        if (command.ExecuteNonQuery() > 0) // Logro el insert
                        {

                            if (int.Parse(xReqcertal) == 1 && int.Parse(xTienecert) == 1)
                            {

                                sSql = " INSERT INTO CAB_DOCUMENTACION_DISCA ";
                                sSql = sSql + " VALUES ";
                                sSql = sSql + "('" + xIDcabecera + "',";
                                sSql = sSql + "" + "9" + ",";
                                sSql = sSql + "" + "GetDate()" + ",";
                                sSql = sSql + "'" + xUsuario + "',";
                                sSql = sSql + "'" + xIdMAx + "',";
                                sSql = sSql + "'" + "NO" + "',";
                                sSql = sSql + "'" + "NO" + "')";
                                command.CommandText = sSql;
                                command.ExecuteNonQuery();

                            }
                        }

                        try
                        {
                            xMensaje = "[{" + '"' + "MENSAJE" + '"' + ":" + '"' + "Se dio de alta correctamente el PM." + "}]";

                        }
                        catch (Exception ex)
                        {
                            xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "NO Se dio de alta correctamente el PM." + "}]";
                        }


                        transaction.Commit();


                    }
                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("nuevadocumentacionpm");
                    xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return xMensaje;
            }

        }
        public string UltimoIdDoc(string sFilial, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string idPto;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "";
                sSQL = sSQL + "SELECT isnull(MAX(ID),0)+1 AS MAXIMO FROM CAB_DOCUMENTACION_PMELIM WITH(NOLOCK)";
                sSQL = sSQL + " WHERE  ID >= " + sFilial + "0000000 AND ID <= " + sFilial + "99999999";


                using (SqlCommand cmd = new SqlCommand(sSQL, con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        if (int.Parse(dt.Rows[0]["MAXIMO"].ToString()) > 0)
                        {
                            idPto = (dt.Rows[0]["MAXIMO"].ToString());
                        }
                        else
                        { idPto = sFilial + "00000000"; }
                    }
                    catch (Exception ex)
                    {
                        idPto = sFilial + "00000000";
                        con.Close();
                    }
                    return idPto;
                }
            }
        }
        public string modificar_pm(string xidpm, string xUsuario, string xIDcabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            //sFilial, fechapm, ID, user, prest 
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string nuevaHab = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (xidpm == null)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "No selecciono ningún Pedido Medico." + '"' + "}]";
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "";
                sSql = sSql + "INSERT INTO CAB_DOCUMENTACION_PMELIM SELECT * FROM CAB_DOCUMENTACION_PM WHERE ID = " + xidpm;


                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("nuevadocumentacion");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;

                    //-------------------------------------


                    if (command.ExecuteNonQuery() > 0) // Logro el insert
                    {

                        sSql = " UPDATE CAB_DOCUMENTACION_PMELIM  ";
                        sSql = sSql + " SET USCRE= '" + xUsuario + "',";
                        sSql = sSql + "FECARGA=" + "GetDate()" + "";
                        sSql = sSql + " WHERE ID='" + xidpm + "'";

                        command.CommandText = sSql;
                        try
                        {
                            if (command.ExecuteNonQuery() > 0) // Logro el insert
                            {
                                sSql = "DELETE FROM CAB_DOCUMENTACION_PM WHERE ID ='" + xidpm + "'";
                                command.CommandText = sSql;
                                command.ExecuteNonQuery();
                                sSql = "DELETE FROM CAB_DOCUMENTACION_DISCA WHERE IDCAB =" + xIDcabecera + " AND IDDOC = 4 AND IDPAPEL ='" + xidpm + "'";
                                command.CommandText = sSql;
                                command.ExecuteNonQuery();
                                sSql = "DELETE FROM CAB_DOCUMENTACION_DISCA WHERE IDCAB =" + xIDcabecera + " AND IDDOC = 9 AND IDPAPEL ='" + xidpm + "'";
                                command.CommandText = sSql;
                                command.ExecuteNonQuery();

                            }

                            xMensaje = "[{" + '"' + "MENSAJE" + '"' + ":" + '"' + "El pedido médico fue modificado." + "}]";

                        }
                        catch (Exception ex)
                        {
                            xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar modificar el pedido médico." + "}]";
                        }


                        transaction.Commit();


                    }
                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("nuevadocumentacion");
                    xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return xMensaje;
            }

        }
        public string cargaEspecialidad( string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string cargEsp = "";


            //61-8076475-01
            //0123456789012

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT * FROM GRAL_ESPECIALIDADDISCA WITH(NOLOCK) ";
                sSql = sSql + " WHERE ESTESP ='A' ";
                sSql = sSql + " ORDER BY CODESP";


                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);


                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            cargEsp = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {



                                cargEsp = cargEsp + "{";
                                cargEsp = cargEsp + '"' + "Especialidad" + '"' + ":" + '"' + dt.Rows[i]["CODESP"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    cargEsp = cargEsp + "},";
                                }
                                else
                                {
                                    cargEsp = cargEsp + "}";
                                };
                            }
                            cargEsp = cargEsp + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cargEsp = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cargEsp = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cargEsp = cargEsp + "{";
                            cargEsp = cargEsp + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cargEsp = cargEsp + "}";
                            }
                            else
                            {
                                cargEsp = cargEsp + "},";
                            };
                        }
                        cargEsp = cargEsp + "]";
                    }
                    con.Close();
                    return cargEsp;

                }
            }

        }
        public string cargaModalidad(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string cargMod = "";

            //61-8076475-01
            //0123456789012

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT * FROM GRAL_MODALIDADESDISCA WITH(NOLOCK)";
                sSql = sSql + " WHERE ESTMOD ='A'";
                sSql = sSql + " ORDER BY CODMOD";


                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);


                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            cargMod = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {



                                cargMod = cargMod + "{";
                                cargMod = cargMod + '"' + "Modalidad" + '"' + ":" + '"' + dt.Rows[i]["CODMOD"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    cargMod = cargMod + "},";
                                }
                                else
                                {
                                    cargMod = cargMod + "}";
                                };
                            }
                            cargMod = cargMod + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cargMod = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cargMod = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cargMod = cargMod + "{";
                            cargMod = cargMod + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cargMod = cargMod + "}";
                            }
                            else
                            {
                                cargMod = cargMod + "},";
                            };
                        }
                        cargMod = cargMod + "]";
                    }
                    con.Close();
                    return cargMod;

                }
            }

        }
        public string presupuestosCargados(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string presupCarg = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (xIDCabecera == null)
            {
                presupCarg = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "No hay seleccionada ninguna cabecera." + '"' + "}]";
                return presupCarg;
            }
            //61-8076475-01
            //0123456789012


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "";
                sSql = sSql + "SELECT presup.ID AS ID,presup.CUIT AS CUIT,isnull(pres.NOMPRESTADOR,'No Existe') AS NOMBREPRESTADOR,";
                sSql = sSql + "presup.FEPTO AS FECHA,presup.CODMOD AS MODALIDAD,presup.CODESP AS ESPECIALIDAD,presup.IMPORTE AS IMPORTE, presup.REQCERTAL AS REQCERTAL";
                sSql = sSql + " FROM CAB_DOCUMENTACION_PRESUP AS presup WITH(NOLOCK)";
                sSql = sSql + " LEFT OUTER JOIN GRAL_PRESTADORES AS pres  ON presup.CUIT = pres.CODPRESTADOR";
                sSql = sSql + " WHERE IDCAB ='" + xIDCabecera + "'";


                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            presupCarg = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                presupCarg = presupCarg + "{";
                                presupCarg = presupCarg + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["ID"].ToString() + '"' + ",";
                                presupCarg = presupCarg + '"' + "CUIT" + '"' + ":" + '"' + dt.Rows[i]["CUIT"].ToString() + '"' + ",";
                                presupCarg = presupCarg + '"' + "NOMBREPRESTADOR" + '"' + ":" + '"' + dt.Rows[i]["NOMBREPRESTADOR"].ToString() + '"' + ",";
                                presupCarg = presupCarg + '"' + "FECHA" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FECHA"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                presupCarg = presupCarg + '"' + "MODALIDAD" + '"' + ":" + '"' + dt.Rows[i]["MODALIDAD"].ToString() + '"' + ",";
                                presupCarg = presupCarg + '"' + "ESPECIALIDAD" + '"' + ":" + '"' + dt.Rows[i]["ESPECIALIDAD"].ToString() + '"' + ",";
                                presupCarg = presupCarg + '"' + "IMPORTE" + '"' + ":" + '"' + dt.Rows[i]["IMPORTE"].ToString() + '"' + ",";
                                presupCarg = presupCarg + '"' + "REQCERTAL" + '"' + ":" + '"' + dt.Rows[i]["REQCERTAL"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    presupCarg = presupCarg + "},";
                                }
                                else
                                {
                                    presupCarg = presupCarg + "}";
                                };
                            }
                            presupCarg = presupCarg + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            presupCarg = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        presupCarg = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            presupCarg = presupCarg + "{";
                            presupCarg = presupCarg + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                presupCarg = presupCarg + "}";
                            }
                            else
                            {
                                presupCarg = presupCarg + "},";
                            };
                        }
                        presupCarg = presupCarg + "]";
                    }
                    con.Close();
                    return presupCarg;

                }
            }

        }
        public string buscarPrestadorPTO(string xCUIT, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string buscarPres = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (xCUIT == null)
            {
                buscarPres = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Debe ingresar un CUIT de Pestador." + '"' + "}]";
                return buscarPres;
            }
            //61-8076475-01
            //0123456789012


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "";
                sSql = sSql + "SELECT CODPRESTADOR,NOMPRESTADOR, Tel1,TEL2";
                sSql = sSql + " FROM GRAL_PRESTADORES  WITH(NOLOCK) ";
                sSql = sSql + " WHERE   CODPRESTADOR ='" + xCUIT + "'";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            buscarPres = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                buscarPres = buscarPres + "{";
                                buscarPres = buscarPres + '"' + "CUIT" + '"' + ":" + '"' + dt.Rows[i]["CODPRESTADOR"].ToString() + '"' + ",";
                                buscarPres = buscarPres + '"' + "NOMBREPRESTADOR" + '"' + ":" + '"' + dt.Rows[i]["NOMPRESTADOR"].ToString() + '"' + ",";
                                buscarPres = buscarPres + '"' + "TEL" + '"' + ":" + '"' + dt.Rows[i]["TEL1"].ToString() + '"' + ",";
                                buscarPres = buscarPres + '"' + "MAIL" + '"' + ":" + '"' + dt.Rows[i]["TEL2"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    buscarPres = buscarPres + "},";
                                }
                                else
                                {
                                    buscarPres = buscarPres + "}";
                                };
                            }
                            buscarPres = buscarPres + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            buscarPres = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "El prestador no existe." + '"' + "}]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        buscarPres = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            buscarPres = buscarPres + "{";
                            buscarPres = buscarPres + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                buscarPres = buscarPres + "}";
                            }
                            else
                            {
                                buscarPres = buscarPres + "},";
                            };
                        }
                        buscarPres = buscarPres + "]";
                    }
                    con.Close();
                    return buscarPres;

                }
            }

        }
        public string altaPresupuesto(string xFilial, string xCUIT, string xIDCabecera, string xUsuario, string xFepresup, string xModalidad, string xEspecialidad, string xImporte, string xUnidadMedida, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            //sFilial, fechapm, ID, user, prest 
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string nuevaHab = "";
            string idPto = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (xCUIT == null || xFepresup == null || xEspecialidad == null)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Error debe ingresar los campos mandatorios." + '"' + "}]";
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                string xIdHabP = UltimoNroIdHabilPresup(xFilial, xModulo, xBase, xUserLog, xFilialLog);
                string xIdHabPel = UltimoNroIdHabilPresupElim(xFilial, xModulo, xBase, xUserLog, xFilialLog);

                if (Double.Parse(xIdHabP) >= double.Parse(xIdHabPel))
                {
                    idPto = xIdHabP;
                }
                else
                {
                    idPto = xIdHabPel;
                }

                sSql = "";
                sSql = sSql + "INSERT INTO CAB_DOCUMENTACION_PRESUP  ";
                sSql = sSql + " VALUES (";
                sSql = sSql + "'" + xCUIT + "',";
                sSql = sSql + "'" + xIDCabecera + "',";
                sSql = sSql + "" + "GetDate()" + ",";
                sSql = sSql + "'" + xUsuario + "',";
                sSql = sSql + "'" + idPto + "',";
                sSql = sSql + "'" + FormatoFecha(xFepresup, "yyyy/mm/dd", true) + "',";
                sSql = sSql + "'" + xModalidad + "',";
                sSql = sSql + "'" + xEspecialidad + "',";
                sSql = sSql + "'" + xImporte + "',";
                sSql = sSql + "'" + xUnidadMedida + "',";
                sSql = sSql + "'" + "NO" + "',";
                sSql = sSql + "'" + "NO" + "')";

                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("nuevadocumentacionpm");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;

                    //-------------------------------------
                    if (command.ExecuteNonQuery() > 0) // Logro el insert
                    {

                        sSql = " INSERT INTO CAB_DOCUMENTACION_DISCA ";
                        sSql = sSql + " VALUES ";
                        sSql = sSql + "('" + xIDCabecera + "',";
                        sSql = sSql + "" + "2" + ",";
                        sSql = sSql + "" + "GetDate()" + ",";
                        sSql = sSql + "'" + xUsuario + "',";
                        sSql = sSql + "'" + idPto + "',";
                        sSql = sSql + "'" + "NO" + "',";
                        sSql = sSql + "'" + "NO" + "')";

                        command.CommandText = sSql;
                        command.ExecuteNonQuery();

                        try
                        {
                            xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "El presupuesto se cargo correctamente." + '"' + "}]";

                        }
                        catch (Exception ex)
                        {
                            xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "El presupuesto NO se cargo." + '"' + "}]";
                        }

                        transaction.Commit();

                    }
                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("nuevadocumentacionpm");
                    xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return xMensaje;
            }

        }
        public string UltimoNroIdHabilPresup(string sFilial, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string idPto;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "SELECT isnull(MAX(ID),0)+1 AS MAXIMO ";
                sSQL = sSQL + " FROM CAB_DOCUMENTACION_PRESUP WITH(NOLOCK) ";
                sSQL = sSQL + "WHERE  ID >=" + sFilial + "00000001 AND ID <=" + sFilial + "99999999";

                using (SqlCommand cmd = new SqlCommand(sSQL, con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        if (Double.Parse(dt.Rows[0]["MAXIMO"].ToString()) > 0)
                        {
                            idPto = (dt.Rows[0]["MAXIMO"].ToString());
                        }
                        else
                        { idPto = sFilial + "00000000"; }
                    }
                    catch (Exception ex)
                    {
                        idPto = sFilial + "00000000";
                        con.Close();
                    }
                    return idPto;
                }
            }
        }
        public string UltimoNroIdHabilPresupElim(string sFilial, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string idPto;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "SELECT isnull(MAX(ID),0)+1 AS MAXIMO ";
                sSQL = sSQL + " FROM CAB_DOCUMENTACION_PRESUPELIM  WITH(NOLOCK) ";
                sSQL = sSQL + " WHERE  ID >=" + sFilial + "00000001 AND ID <=" + sFilial + "99999999";

                using (SqlCommand cmd = new SqlCommand(sSQL, con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        if (Double.Parse(dt.Rows[0]["MAXIMO"].ToString()) > 0)
                        {
                            idPto = (dt.Rows[0]["MAXIMO"].ToString());
                        }
                        else
                        { idPto = sFilial + "00000000"; }
                    }
                    catch (Exception ex)
                    {
                        idPto = sFilial + "00000000";
                        con.Close();
                    }
                    return idPto;
                }
            }
        }
        public string bajaPresupuesto(string xIDpresupuesto, string xUsuario, string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            //sFilial, fechapm, ID, user, prest 
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string nuevaHab = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (xIDpresupuesto == null || xUsuario == null || xIDCabecera == null)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Debe indicar el registro a modificar." + '"' + "}]";
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "";
                sSql = sSql + "INSERT INTO CAB_DOCUMENTACION_PRESUPELIM SELECT * FROM CAB_DOCUMENTACION_PRESUP WHERE ID = " + xIDpresupuesto;

                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("nuevadocumentacion");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;

                    //-------------------------------------


                    if (command.ExecuteNonQuery() > 0) // Logro el insert
                    {

                        sSql = " UPDATE CAB_DOCUMENTACION_PRESUPELIM  ";
                        sSql = sSql + " SET USCRE= '" + xUsuario + "',";
                        sSql = sSql + "FECARGA=" + "GetDate()" + "";
                        sSql = sSql + " WHERE ID='" + xIDpresupuesto + "'";

                        command.CommandText = sSql;
                        try
                        {
                            if (command.ExecuteNonQuery() > 0) // Logro el insert
                            {

                                sSql = "DELETE FROM CAB_DOCUMENTACION_PRESUP WHERE ID = '" + xIDpresupuesto + "'";
                                command.CommandText = sSql;
                                command.ExecuteNonQuery();
                                sSql = "DELETE FROM CAB_DOCUMENTACION_DISCA WHERE IDPAPEL =" + xIDpresupuesto + " AND IDCAB = " + xIDCabecera + " AND IDDOC = 2";
                                command.CommandText = sSql;
                                command.ExecuteNonQuery();

                            }

                            xMensaje = "[{" + '"' + "MENSAJE" + '"' + ":" + '"' + "El pedido médico fue modificado." + "}]";

                        }
                        catch (Exception ex)
                        {
                            xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar modificar el pedido médico." + "}]";
                        }


                        transaction.Commit();
                        //Invoco al metodo cargar_habilitaciones 
                        String a = this.presupuestosCargados(xIDCabecera, xModulo,  xBase,  xUserLog,  xFilialLog);
                        xMensaje = a;

                    }
                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("nuevadocumentacion");
                    xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return xMensaje;
            }

        }
        bool EsStrNuloBlanco(string xValor)
        {
            if (xValor == null || xValor == "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        string ValidarDatosConexion(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string xMensaje = "El/los campo/s ";

            if (EsStrNuloBlanco(xModulo) == true)
            {
                xMensaje = xMensaje + " Módulo,";
            }
            //else
            //{
            if (EsStrNuloBlanco(xBase) == true)
            {
                xMensaje = xMensaje + " Base,";
            }
            //else
            //{
            if (EsStrNuloBlanco(xUserLog) == true)
            {
                xMensaje = xMensaje + " Usuario de Login,";
            }
            //else
            //{
            if (EsStrNuloBlanco(xFilialLog) == true)
            {
                xMensaje = xMensaje + " Filial de Login";
            }


            if (xMensaje.Length > 15)
            {
                xMensaje = xMensaje + " deben ser informados.";
                return xMensaje;
            }
            return "";
        }
    }
}





