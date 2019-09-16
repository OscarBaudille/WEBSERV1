using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Services;
using System.Web.UI.HtmlControls;
using WcfServiceAPE;


namespace WcfServiceAPE
{
    [ServiceBehavior(Namespace = "http://ar.com.osde/osgapeservice/", Name = "OSGAPEBackendService")]
    public class WS3 : Subgrillas
    {
        string Subgrillas.cons_con_presupuestos(string xIDCABECERA, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();

            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            string jPresup = "";
            string xFlag = "";
            // bool AgregarHabNueva = false;
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string xFprest = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            //AgregarHabNueva = false;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "";
                sSql = sSql + "SELECT RENGLONES.ID,RENGLONES.CODPRESTADOR, CASE WHEN NOMBRE IS NULL THEN ";
                sSql = sSql + " CASE WHEN APELLIDO IS NULL THEN '' ELSE APELLIDO END ELSE NOMBRE + ' ' + APELLIDO END AS PRESTADOR, ";
                sSql = sSql + " CASE WHEN NROTRAMITE IS NULL then 'R' else 'T' END AS RT,TIPO,";
                sSql = sSql + " CASE WHEN NROTRAMITE IS NULL then ORDENPRACTICA ";
                sSql = sSql + " else NROTRAMITE END AS TRAMITE, FEPREST, NROFC, IMPORTE, NRORC";
                sSql = sSql + " FROM RENGLONES WITH(NOLOCK) INNER JOIN GRAL_PRESTADORES WITH(NOLOCK) ON RENGLONES.CODPRESTADOR = GRAL_PRESTADORES.CUIT";
                sSql = sSql + " WHERE RENGLONES.CODPRESTADOR = ANY (SELECT CUIT FROM CAB_DOCUMENTACION_DISCA WITH(NOLOCK) INNER JOIN CAB_DOCUMENTACION_PRESUP WITH(NOLOCK) ON ";
                sSql = sSql + " CAB_DOCUMENTACION_DISCA.IDPAPEL = CAB_DOCUMENTACION_PRESUP.ID WHERE ";
                sSql = sSql + " IDDOC = 2 AND CAB_DOCUMENTACION_DISCA.IDCAB = " + xIDCABECERA + ")";
                sSql = sSql + " AND (TIPO <> 'T' AND RENGLONES.CODPRESTADOR = ANY (SELECT CUIT FROM CAB_DOCUMENTACION_DISCA WITH(NOLOCK) ";
                sSql = sSql + " INNER JOIN GRAL_HABILITACIONES ON  CAB_DOCUMENTACION_DISCA.IDPAPEL = GRAL_HABILITACIONES.IDHABIL WHERE  IDDOC = 3 ";
                sSql = sSql + " AND IDCAB = " + xIDCABECERA + ") OR (TIPO = 'T' AND IDCAB =" + xIDCABECERA + ")) ";
                sSql = sSql + " AND IDCAB = " + xIDCABECERA;
                sSql = sSql + " ORDER BY RENGLONES.CODPRESTADOR, FEPREST";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int nroFc = 0;
                        int Contador = dt.Rows.Count;

                        if (dt.Rows.Count > 0)
                        {
                            jPresup = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                if (dt.Rows[i]["nroFc"].ToString() != dt.Rows[i]["nroRc"].ToString())
                                {
                                    if (dt.Rows[i]["nroRc"].ToString() != "")
                                    {
                                        xFlag = "";
                                    }
                                    else
                                    {
                                        xFlag = "R";
                                    }
                                }

                                jPresup = jPresup + "{";
                                jPresup = jPresup + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["ID"].ToString() + '"' + ",";
                                jPresup = jPresup + '"' + "RT" + '"' + ":" + '"' + dt.Rows[i]["RT"].ToString() + '"' + ",";
                                jPresup = jPresup + '"' + "PRESTADOR" + '"' + ":" + '"' + dt.Rows[i]["PRESTADOR"].ToString() + '"' + ",";
                                jPresup = jPresup + '"' + "Nro_Doc" + '"' + ":" + '"' + dt.Rows[i]["TRAMITE"].ToString() + '"' + ",";

                                xFprest = FormatoFecha2(dt.Rows[i]["feprest"].ToString(), "dd/mm/yyyy", false);
                                //xFprest = xFprest.Substring(0, 10);

                                jPresup = jPresup + '"' + "Fecha_Prest" + '"' + ":" + '"' + xFprest + '"' + ",";
                                jPresup = jPresup + '"' + "Nro_Fact" + '"' + ":" + '"' + dt.Rows[i]["nroFc"].ToString() + '"' + ",";
                                jPresup = jPresup + '"' + "Importe" + '"' + ":" + '"' + Convert.ToDouble(dt.Rows[i]["Importe"].ToString()) + '"' + ",";
                                jPresup = jPresup + '"' + "flag" + '"' + ":" + '"' + xFlag + '"';


                                if (i < Contador - 1)
                                {
                                    jPresup = jPresup + "},";
                                }
                                else
                                {
                                    jPresup = jPresup + "}";
                                };
                            }
                            jPresup = jPresup + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            jPresup = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jPresup = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jPresup = jPresup + "{";
                            jPresup = jPresup + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jPresup = jPresup + "}";
                            }
                            else
                            {
                                jPresup = jPresup + "},";
                            };
                        }
                        jPresup = jPresup + "]";
                    }
                    con.Close();
                    return jPresup;
                }
            }
        }
        string Subgrillas.cons_sin_presupuestos(string xIDCABECERA, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();

            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            string jsPresup = "";
            string xFlag = "";
            // bool AgregarHabNueva = false;
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string xFprest = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            //AgregarHabNueva = false;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "";
                sSql = sSql + "SELECT RENGLONES.ID,RENGLONES.CODPRESTADOR, ";
                sSql = sSql + " CASE WHEN NOMBRE IS NULL THEN CASE WHEN APELLIDO IS NULL ";
                sSql = sSql + " THEN '' ELSE APELLIDO END ELSE NOMBRE + ' ' + APELLIDO END AS PRESTADOR, ";
                sSql = sSql + " CASE WHEN NROTRAMITE IS NULL then 'R' else 'T' END AS RT,TIPO, CASE WHEN NROTRAMITE IS NULL ";
                sSql = sSql + " then ORDENPRACTICA else NROTRAMITE END AS TRAMITE, FEPREST, NROFC, IMPORTE, NRORC ";
                sSql = sSql + " FROM RENGLONES WITH(NOLOCK) INNER JOIN GRAL_PRESTADORES WITH(NOLOCK) ON ";
                sSql = sSql + " RENGLONES.CODPRESTADOR = GRAL_PRESTADORES.CUIT  WHERE IDCAB = " + xIDCABECERA + " AND RENGLONES.CODPRESTADOR NOT IN ";
                sSql = sSql + " (SELECT CUIT FROM CAB_DOCUMENTACION_DISCA WITH(NOLOCK) INNER JOIN CAB_DOCUMENTACION_PRESUP WITH(NOLOCK)";
                sSql = sSql + " ON  CAB_DOCUMENTACION_DISCA.IDPAPEL = CAB_DOCUMENTACION_PRESUP.ID WHERE  IDDOC = 2 ";
                sSql = sSql + " AND CAB_DOCUMENTACION_DISCA.IDCAB = " + xIDCABECERA + ") ";
                sSql = sSql + " UNION";
                sSql = sSql + " (SELECT RENGLONES.ID,RENGLONES.CODPRESTADOR, ";
                sSql = sSql + " CASE WHEN NOMBRE IS NULL THEN CASE WHEN APELLIDO IS NULL THEN '' ELSE APELLIDO END ";
                sSql = sSql + " ELSE NOMBRE + ' ' + APELLIDO END AS PRESTADOR, ";
                sSql = sSql + " CASE WHEN NROTRAMITE IS NULL then 'R' else 'T' END AS RT,TIPO, ";
                sSql = sSql + " CASE WHEN NROTRAMITE IS NULL then ORDENPRACTICA else NROTRAMITE END AS TRAMITE, FEPREST, NROFC, IMPORTE, NRORC ";
                sSql = sSql + " FROM RENGLONES WITH(NOLOCK) INNER JOIN GRAL_PRESTADORES WITH(NOLOCK) ";
                sSql = sSql + " ON RENGLONES.CODPRESTADOR = GRAL_PRESTADORES.CUIT  WHERE IDCAB = " + xIDCABECERA + " AND TIPO <> 'T' ";
                sSql = sSql + " AND RENGLONES.CODPRESTADOR NOT IN (SELECT CUIT FROM CAB_DOCUMENTACION_DISCA WITH(NOLOCK)  ";
                sSql = sSql + " INNER JOIN GRAL_HABILITACIONES WITH(NOLOCK) ON  CAB_DOCUMENTACION_DISCA.IDPAPEL = GRAL_HABILITACIONES.IDHABIL ";
                sSql = sSql + " WHERE  IDDOC = 3 AND IDCAB = " + xIDCABECERA + "))";
                sSql = sSql + " ORDER BY RENGLONES.CODPRESTADOR, FEPREST";


                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int nroFc = 0;
                        int Contador = dt.Rows.Count;

                        if (dt.Rows.Count > 0)
                        {
                            jsPresup = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (dt.Rows[i]["nroFc"].ToString() != dt.Rows[i]["nroRc"].ToString())
                                {
                                    if (dt.Rows[i]["nroRc"].ToString() != "")
                                    {
                                        xFlag = "";
                                    }
                                    else
                                    {
                                        xFlag = "R";
                                    }
                                }

                                jsPresup = jsPresup + "{";
                                jsPresup = jsPresup + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["ID"].ToString() + '"' + ",";
                                jsPresup = jsPresup + '"' + "RT" + '"' + ":" + '"' + dt.Rows[i]["RT"].ToString() + '"' + ",";
                                jsPresup = jsPresup + '"' + "PRESTADOR" + '"' + ":" + '"' + dt.Rows[i]["PRESTADOR"].ToString() + '"' + ",";
                                jsPresup = jsPresup + '"' + "Nro_Doc" + '"' + ":" + '"' + dt.Rows[i]["TRAMITE"].ToString() + '"' + ",";

                                xFprest = FormatoFecha2(dt.Rows[i]["feprest"].ToString(), "dd/mm/yyyy", false);
                                //xFprest = xFprest.Substring(0, 10);

                                jsPresup = jsPresup + '"' + "Fecha_Prest" + '"' + ":" + '"' + xFprest + '"' + ",";
                                jsPresup = jsPresup + '"' + "Nro_Fact" + '"' + ":" + '"' + dt.Rows[i]["nroFc"].ToString() + '"' + ",";
                                jsPresup = jsPresup + '"' + "Importe" + '"' + ":" + '"' + Convert.ToDouble(dt.Rows[i]["Importe"].ToString()) + '"' + ",";
                                jsPresup = jsPresup + '"' + "flag" + '"' + ":" + '"' + xFlag + '"';

                                if (i < Contador - 1)
                                {
                                    jsPresup = jsPresup + "},";
                                }
                                else
                                {
                                    jsPresup = jsPresup + "}";
                                };
                            }
                            jsPresup = jsPresup + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            jsPresup = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jsPresup = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jsPresup = jsPresup + "{";
                            jsPresup = jsPresup + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jsPresup = jsPresup + "}";
                            }
                            else
                            {
                                jsPresup = jsPresup + "},";
                            };
                        }
                        jsPresup = jsPresup + "]";
                    }
                    con.Close();
                    return jsPresup;
                }
            }
        }

        string Subgrillas.doc_Dicapacidad(string xIDCABECERA, string xPeriodo, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();

            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            string jDisca = "";
            string xFlag = "";

            // bool AgregarHabNueva = false;
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();

            int chkAutTit = 0;
            int chkprespuesto = 0;
            int chkhabilitacion = 0;
            int chkPM = 0;
            int chkRHC = 0;
            int chkPlTransp = 0;
            int chkCertDisc = 0;
            int chkcarta = 0;
            int chkcertalumno = 0;
            int chkcontacto = 0;

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                
                sSql = "SELECT iddoc,idpapel FROM CAB_DOCUMENTACION_DISCA WITH(NOLOCK) ";
                sSql = sSql + " WHERE IDCAB = " + xIDCABECERA;
                sSql = sSql + " GROUP BY IDDOC,IDPAPEL ";
                sSql = sSql + " ORDER BY IDDOC";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int nroFc = 0;
                        int Contador = dt.Rows.Count;
                        if (nroFc != Contador)
                        {
                            if (Contador != 0)
                            {
                                xFlag = "X";
                            }
                            else
                            {
                                xFlag = "R";
                            }
                        }

                        if (dt.Rows.Count > 0)
                        {
                            jDisca = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                switch (dt.Rows[i]["IDDOC"].ToString())
                                {
                                    case "1":
                                        chkAutTit = 1;
                                        break;
                                    case "2":
                                        chkprespuesto = 1;
                                        break;
                                    case "3":
                                        chkhabilitacion = 1;
                                        break;
                                    case "4":
                                        chkPM = 1;
                                        break;
                                    case "5":
                                        chkRHC = 1;
                                        break;
                                    case "6":
                                        chkPlTransp = 1;
                                        break;
                                    case "7":
                                        string xIdpapel = dt.Rows[i]["IDPAPEL"].ToString();
                                        chkCertDisc = VeoSiTieneCertificados(xIdpapel, xPeriodo, xModulo,  xBase,  xUserLog,  xFilialLog);
                                        break;
                                    case "8":
                                        chkcarta = 1;
                                        break;
                                    case "9":
                                        chkcertalumno = 1;
                                        break;
                                    case "10":
                                        chkcontacto = 1;
                                        break;
                                }
                                if (i == dt.Rows.Count - 1)
                                {

                                    jDisca = jDisca + "{";
                                    jDisca = jDisca + '"' + "Notatitular" + '"' + ":" + '"' + chkAutTit + '"' + ",";
                                    jDisca = jDisca + '"' + "Presupuesto" + '"' + ":" + '"' + chkprespuesto + '"' + ",";
                                    jDisca = jDisca + '"' + "Habilitacion" + '"' + ":" + '"' + chkhabilitacion + '"' + ",";
                                    jDisca = jDisca + '"' + "Pedidomedico" + '"' + ":" + '"' + chkPM + '"' + ",";
                                    jDisca = jDisca + '"' + "ResuHC" + '"' + ":" + '"' + chkRHC + '"' + ",";
                                    jDisca = jDisca + '"' + "Transporte" + '"' + ":" + '"' + chkPlTransp + '"' + ",";
                                    jDisca = jDisca + '"' + "cartDisca" + '"' + ":" + '"' + chkCertDisc + '"' + ",";
                                    jDisca = jDisca + '"' + "cartaRec" + '"' + ":" + '"' + chkcarta + '"' + ",";
                                    jDisca = jDisca + '"' + "certalum" + '"' + ":" + '"' + chkcertalumno + '"' + ",";
                                    jDisca = jDisca + '"' + "contacto" + '"' + ":" + '"' + chkcontacto + '"';

                                    if (i < Contador - 1)
                                    {
                                        jDisca = jDisca + "},";
                                    }
                                    else
                                    {
                                        jDisca = jDisca + "}";
                                    };

                                    chkAutTit = 0;
                                    chkprespuesto = 0;
                                    chkhabilitacion = 0;
                                    chkPM = 0;
                                    chkRHC = 0;
                                    chkPlTransp = 0;
                                    chkCertDisc = 0;
                                    chkcarta = 0;
                                    chkcertalumno = 0;
                                    chkcontacto = 0;
                                }
                            }

                            jDisca = jDisca + "]";


                        }
                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            jDisca = jDisca + "[{";
                            jDisca = jDisca + '"' + "Notatitular" + '"' + ":" + '"' + "0" + '"' + ",";
                            jDisca = jDisca + '"' + "Presupuesto" + '"' + ":" + '"' + "0" + '"' + ",";
                            jDisca = jDisca + '"' + "Habilitacion" + '"' + ":" + '"' + "0" + '"' + ",";
                            jDisca = jDisca + '"' + "Pedidomedico" + '"' + ":" + '"' + "0" + '"' + ",";
                            jDisca = jDisca + '"' + "ResuHC" + '"' + ":" + '"' + "0" + '"' + ",";
                            jDisca = jDisca + '"' + "Transporte" + '"' + ":" + '"' + "0" + '"' + ",";
                            jDisca = jDisca + '"' + "cartDisca" + '"' + ":" + '"' + "0" + '"' + ",";
                            jDisca = jDisca + '"' + "cartaRec" + '"' + ":" + '"' + "0" + '"' + ",";
                            jDisca = jDisca + '"' + "certalum" + '"' + ":" + '"' + "0" + '"' + ",";
                            jDisca = jDisca + '"' + "contacto" + '"' + ":" + '"' + "0" + '"' + "}]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jDisca = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jDisca = jDisca + "{";
                            jDisca = jDisca + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jDisca = jDisca + "}";
                            }
                            else
                            {
                                jDisca = jDisca + "},";
                            };
                        }
                        jDisca = jDisca + "]";
                    }
                    con.Close();
                    return jDisca;
                }
            }
        }
        string Subgrillas.datos_ape(string xIDCABECERA, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();

            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            string jDatosApe = "";
            string xFlag = "";
            string FechaPrest = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "";
                sSql = sSql + "SELECT isnull(NROEXPDTE,'') AS NROEXPDTE, isnull(FEPRES,'') AS FEPRES, isnull(MONTOPRES,'') AS MONTOPRES, isnull(CAJA,'') AS CAJA, isnull(OBSERV,'') AS OBSERV,isnull(MOTIVOBAJAADM,'') AS MOTIVOBAJAADM,isnull(MOTIVOBAJACBLE,'') AS MOTIVOBAJACBLE FROM CABECERAS WITH(NOLOCK) ";
                sSql = sSql + " WHERE ID =" + xIDCABECERA;

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
                            jDatosApe = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                if (dt.Rows[i]["FEPRES"].ToString() == "" || dt.Rows[i]["FEPRES"].ToString() == null || dt.Rows[i]["FEPRES"].ToString() == "NULL")
                                {
                                    FechaPrest = "";
                                }
                                else
                                {
                                        FechaPrest = FormatoFecha2(dt.Rows[i]["FEPRES"].ToString(), "dd/mm/yyyy", false);
                                }

                                jDatosApe = jDatosApe + "{";
                                jDatosApe = jDatosApe + '"' + "bajaadm" + '"' + ":" + '"' + dt.Rows[i]["MOTIVOBAJAADM"].ToString() + '"' + ",";
                                jDatosApe = jDatosApe + '"' + "bajactble" + '"' + ":" + '"' + dt.Rows[i]["MOTIVOBAJACBLE"].ToString() + '"' + ",";
                                jDatosApe = jDatosApe + '"' + "nroexpdt" + '"' + ":" + '"' + dt.Rows[i]["NROEXPDTE"].ToString() + '"' + ",";
                                jDatosApe = jDatosApe + '"' + "ubicacion" + '"' + ":" + '"' + dt.Rows[i]["CAJA"].ToString() + '"' + ",";
                                jDatosApe = jDatosApe + '"' + "fepresentado" + '"' + ":" + '"' + FechaPrest + '"' + ",";
                                if (dt.Rows[i]["MONTOPRES"].ToString() == "0")
                                {
                                    jDatosApe = jDatosApe + '"' + "monto" + '"' + ":" + '"' + "" + '"' + ",";
                                }
                                else
                                {
                                    jDatosApe = jDatosApe + '"' + "monto" + '"' + ":" + '"' + dt.Rows[i]["MONTOPRES"].ToString() + '"' + ",";
                                }
                                jDatosApe = jDatosApe + '"' + "obs" + '"' + ":" + '"' + dt.Rows[i]["OBSERV"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    jDatosApe = jDatosApe + "},";
                                }
                                else
                                {
                                    jDatosApe = jDatosApe + "}";
                                };
                            }
                            jDatosApe = jDatosApe + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            jDatosApe = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jDatosApe = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jDatosApe = jDatosApe + "{";
                            jDatosApe = jDatosApe + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jDatosApe = jDatosApe + "}";
                            }
                            else
                            {
                                jDatosApe = jDatosApe + "},";
                            };
                        }
                        jDatosApe = jDatosApe + "]";
                    }
                    con.Close();
                    return jDatosApe;
                }
            }
        }

        string Subgrillas.sgrilla_registronac(string xIDCABECERA, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();

            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            string jsRegNac = "";
            string xFlag = "";
            string xMarca1 = "";
            string xMarca2 = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT codprestador,prestador,max(marca1) as marca1,max(marca2) as marca2 from ";
                sSql = sSql + "(SELECT RENGLONES.CODPRESTADOR,CASE WHEN NOMBRE IS NULL THEN CASE WHEN APELLIDO IS NULL THEN '' ELSE APELLIDO END ELSE NOMBRE + ' ' + APELLIDO END AS PRESTADOR,'' as marca1,(SELECT case when count(*)>0 then 'F' ELSE '' end ) as marca2 FROM RENGLONES WITH(NOLOCK) INNER JOIN GRAL_PRESTADORES WITH(NOLOCK) ON RENGLONES.CODPRESTADOR = GRAL_PRESTADORES.CUIT  WHERE IDCAB =" + xIDCABECERA + " AND TIPO <> 'T' AND RENGLONES.CODPRESTADOR NOT IN (SELECT CUIT FROM CAB_DOCUMENTACION_DISCA WITH(NOLOCK)  INNER JOIN GRAL_HABILITACIONES WITH(NOLOCK) ON  CAB_DOCUMENTACION_DISCA.IDPAPEL = GRAL_HABILITACIONES.IDHABIL WHERE  IDDOC = 3 AND IDCAB = " + xIDCABECERA + ") GROUP BY RENGLONES.CODPRESTADOR, NOMBRE, APELLIDO";
                sSql = sSql + " UNION ALL ";
                sSql = sSql + "SELECT RENGLONES.CODPRESTADOR ,CASE WHEN NOMBRE IS NULL THEN CASE WHEN APELLIDO IS NULL THEN '' ELSE APELLIDO END ELSE NOMBRE + ' ' + APELLIDO END AS PRESTADOR,(SELECT case when count(*)>0 then 'F' ELSE '' end ) as marca1,'' as marca2 FROM RENGLONES WITH(NOLOCK) INNER JOIN GRAL_PRESTADORES WITH(NOLOCK) ON RENGLONES.CODPRESTADOR = GRAL_PRESTADORES.CUIT  WHERE IDCAB = " + xIDCABECERA + " AND RENGLONES.CODPRESTADOR NOT IN (SELECT CUIT FROM CAB_DOCUMENTACION_DISCA WITH(NOLOCK) INNER JOIN CAB_DOCUMENTACION_PRESUP WITH(NOLOCK) ON  CAB_DOCUMENTACION_DISCA.IDPAPEL = CAB_DOCUMENTACION_PRESUP.ID WHERE  IDDOC = 2 AND CAB_DOCUMENTACION_DISCA.IDCAB = " + xIDCABECERA + ") GROUP BY RENGLONES.CODPRESTADOR,NOMBRE,APELLIDO)";
                sSql = sSql + " AS TEMPORAL";
                sSql = sSql + " GROUP BY codprestador,prestador";

                //sSql = sSql + "SELECT RENGLONES.CODPRESTADOR ,CASE WHEN NOMBRE IS NULL THEN ";
                //sSql = sSql + " CASE WHEN APELLIDO IS NULL THEN '' ELSE APELLIDO END ELSE NOMBRE + ' ' + APELLIDO END AS PRESTADOR ";
                //sSql = sSql + " ,(";
                //sSql = sSql +" SELECT case when count(*)>0 then 'F' ELSE '' end as Registro ";
                //sSql = sSql +" FROM RENGLONES as renglones2 WITH(NOLOCK) INNER JOIN GRAL_PRESTADORES WITH(NOLOCK) ";
                //sSql = sSql + " ON RENGLONES2.CODPRESTADOR = GRAL_PRESTADORES.CUIT  WHERE IDCAB = " + xIDCABECERA + " AND TIPO <> 'T' ";
                //sSql = sSql +" AND RENGLONES2.CODPRESTADOR NOT IN (SELECT CUIT FROM CAB_DOCUMENTACION_DISCA WITH(NOLOCK)  ";
                //sSql = sSql +" INNER JOIN GRAL_HABILITACIONES WITH(NOLOCK) ON  CAB_DOCUMENTACION_DISCA.IDPAPEL = GRAL_HABILITACIONES.IDHABIL ";
                //sSql = sSql + " WHERE  IDDOC = 3 AND IDCAB = " + xIDCABECERA + ")  and RENGLONES2.CODPRESTADOR=RENGLONES.CODPRESTADOR";
                //sSql = sSql + " ) as registro";
                //sSql = sSql + " FROM RENGLONES WITH(NOLOCK) INNER JOIN GRAL_PRESTADORES WITH(NOLOCK) ";
                //sSql = sSql + " ON RENGLONES.CODPRESTADOR = GRAL_PRESTADORES.CUIT  WHERE IDCAB = " + xIDCABECERA;
                //sSql = sSql + " AND RENGLONES.CODPRESTADOR NOT IN (SELECT CUIT FROM CAB_DOCUMENTACION_DISCA WITH(NOLOCK) ";
                //sSql = sSql + " INNER JOIN CAB_DOCUMENTACION_PRESUP WITH(NOLOCK) ";
                //sSql = sSql + " ON  CAB_DOCUMENTACION_DISCA.IDPAPEL = CAB_DOCUMENTACION_PRESUP.ID ";
                //sSql = sSql + " WHERE  IDDOC = 2 AND CAB_DOCUMENTACION_DISCA.IDCAB = " + xIDCABECERA + ") ";
                //sSql = sSql + " GROUP BY RENGLONES.CODPRESTADOR,NOMBRE,APELLIDO";


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
                            jsRegNac = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                //switch (dt.Rows[i]["marca1"].ToString())
                                //{
                                //    case "F":
                                //        xMarca1 = "F";
                                //        break;


                                //}
                                //switch (dt.Rows[i]["marca2"].ToString())
                                //{
                                //    case "F":
                                //        xMarca2 = "F";
                                //        break;


                                //}
                                //if (i == dt.Rows.Count - 1)
                                //{
                                jsRegNac = jsRegNac + "{";
                                jsRegNac = jsRegNac + '"' + "codprestador" + '"' + ":" + '"' + dt.Rows[i]["CODPRESTADOR"].ToString() + '"' + ",";
                                jsRegNac = jsRegNac + '"' + "Prestador" + '"' + ":" + '"' + dt.Rows[i]["PRESTADOR"].ToString() + '"' + ",";
                                jsRegNac = jsRegNac + '"' + "Presupuesto" + '"' + ":" + '"' + dt.Rows[i]["marca1"].ToString() + '"' + ",";
                                jsRegNac = jsRegNac + '"' + "regnac" + '"' + ":" + '"' + dt.Rows[i]["marca2"].ToString() + '"';


                                if (i < Contador - 1)
                                {
                                    jsRegNac = jsRegNac + "},";
                                }
                                else
                                {
                                    jsRegNac = jsRegNac + "}";
                                };
                                //}
                            }
                            jsRegNac = jsRegNac + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            jsRegNac = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jsRegNac = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jsRegNac = jsRegNac + "{";
                            jsRegNac = jsRegNac + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jsRegNac = jsRegNac + "}";
                            }
                            else
                            {
                                jsRegNac = jsRegNac + "},";
                            };
                        }
                        jsRegNac = jsRegNac + "]";
                    }
                    con.Close();
                    return jsRegNac;
                }
            }
        }
        string Subgrillas.sgrilla_sinconsumo(string xIDCABECERA, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();

            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            string jsRegNac = "";
            string xFlag = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "";
                sSql = sSql + "SELECT CUIT, MAX(IDDOC) AS MAXIMO, MIN(IDDOC) AS MINIMO FROM CAB_DOCUMENTACION_DISCA WITH(NOLOCK) ";
                sSql = sSql + " INNER JOIN CAB_DOCUMENTACION_PRESUP WITH(NOLOCK) ";
                sSql = sSql + " ON  CAB_DOCUMENTACION_DISCA.IDPAPEL = CAB_DOCUMENTACION_PRESUP.ID ";
                sSql = sSql + " WHERE CAB_DOCUMENTACION_DISCA.IDCAB = " + xIDCABECERA + " AND IDDOC = 2 OR  CAB_DOCUMENTACION_DISCA.IDCAB = " + xIDCABECERA;
                sSql = sSql + " AND IDDOC = 4 ";
                sSql = sSql + " GROUP BY CUIT  HAVING MAX(IDDOC) =4 AND MIN(IDDOC) = 2";

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
                            jsRegNac = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                jsRegNac = jsRegNac + "{";
                                jsRegNac = jsRegNac + '"' + "cuit" + '"' + ":" + '"' + dt.Rows[i]["cuit"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    jsRegNac = jsRegNac + "},";
                                }
                                else
                                {
                                    jsRegNac = jsRegNac + "}";
                                };
                            }
                            jsRegNac = jsRegNac + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            jsRegNac = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jsRegNac = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jsRegNac = jsRegNac + "{";
                            jsRegNac = jsRegNac + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jsRegNac = jsRegNac + "}";
                            }
                            else
                            {
                                jsRegNac = jsRegNac + "},";
                            };
                        }
                        jsRegNac = jsRegNac + "]";
                    }
                    con.Close();
                    return jsRegNac;
                }
            }
        }
        public string FormatoFecha2(string xDate, String xFormat, bool xTime)
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
                return xDate = null;
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
                xDate = xDate + " 12:00:00";
            }
            return xDate;
        }
        public int VeoSiTieneCertificados(string xIDPAPEL, string xPeriodo,string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            int idHab = 0;
            string sSQL;
            DateTime xFvto;
            string xFvtoChar;
            DateTime xPeriodoDate;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "";
                sSQL = sSQL + "SELECT * FROM GRAL_CERTIFICADOS WITH(NOLOCK) WHERE IDCERT =" + xIDPAPEL;


                using (SqlCommand cmd = new SqlCommand(sSQL, con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            xFvtoChar = dt.Rows[0]["feVto"].ToString().Substring(0, 10);
                            xFvto = DateTime.Parse(FormatoFecha2(xFvtoChar, "dd/mm/yyyy", false));
                            xPeriodoDate = DateTime.Parse(FormatoFecha2(xPeriodo.Substring(13, 10), "dd/mm/yyyy", false));

                            if (xFvto < xPeriodoDate)
                            {
                                idHab = 2;
                            }
                            else
                            {
                                idHab = 1;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        idHab = 0;
                        con.Close();
                    }
                    return idHab;
                }

            }


        }
        // SubGrillas para registros distintos de DISCAPACIDAD--------------------------------------------------------------------------
        string Subgrillas.consumos_Adm(string xidcabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            //int i=0;
            string Cadm = "";
            string xFlag = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT ORDENPRACTICA, IMPORTE, FEPREST, NROTRAMITE, FECIERRE, NOMPRESTADOR,  NROFC, FECHAFC, NRORC, FERC,FECHAFC ";
                sSql = sSql + " FROM RENGLONES WITH(NOLOCK)";
                sSql = sSql + " WHERE IDCAB =" + xidcabecera;
                sSql = sSql + " ORDER BY FEPREST";

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
                            Cadm = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (dt.Rows[i]["nroFc"].ToString() != dt.Rows[i]["nroRc"].ToString())
                                {
                                    if (dt.Rows[i]["nroRc"].ToString() != "")
                                    {
                                        xFlag = "";
                                    }
                                    else
                                    {
                                        xFlag = "R";
                                    }
                                }


                                Cadm = Cadm + "{";
                                Cadm = Cadm + '"' + "Fecha_Fc" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[i]["FECHAFC"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                Cadm = Cadm + '"' + "Fecha_Prest" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[i]["fePrest"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                Cadm = Cadm + '"' + "Nro_Rc" + '"' + ":" + '"' + dt.Rows[i]["nroRc"].ToString() + '"' + ",";
                                Cadm = Cadm + '"' + "Fecha_Rc" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[i]["feRc"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                Cadm = Cadm + '"' + "Prestador" + '"' + ":" + '"' + dt.Rows[i]["NOMPRESTADOR"].ToString() + '"' + ",";
                                Cadm = Cadm + '"' + "Nro_Factura" + '"' + ":" + '"' + dt.Rows[i]["nroFc"].ToString() + '"' + ",";
                                Cadm = Cadm + '"' + "Practica" + '"' + ":" + '"' + dt.Rows[i]["ORDENPRACTICA"].ToString() + '"' + ",";
                                Cadm = Cadm + '"' + "Nro_Tramite" + '"' + ":" + '"' + dt.Rows[i]["NROTRAMITE"].ToString() + '"' + ",";
                                Cadm = Cadm + '"' + "Fecha_Cierre" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[i]["FECIERRE"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                Cadm = Cadm + '"' + "Importe" + '"' + ":" + '"' + dt.Rows[i]["Importe"].ToString() + '"' + ",";
                                Cadm = Cadm + '"' + "Flag" + '"' + ":" + '"' + xFlag + '"';

                                if (i < Contador - 1)
                                {
                                    Cadm = Cadm + "},";
                                }
                                else
                                {
                                    Cadm = Cadm + "}";
                                };
                            }
                            Cadm = Cadm + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            Cadm = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        Cadm = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            Cadm = Cadm + "{";
                            Cadm = Cadm + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                Cadm = Cadm + "}";
                            }
                            else
                            {
                                Cadm = Cadm + "},";
                            };
                        }
                        Cadm = Cadm + "]";
                    }
                    con.Close();
                    return Cadm;

                }
            }
        }


        public string DocumentosAdmS(string xIDCABECERA, string xCONCEPTO, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            int i = 0;
            string Dadm = "";
            string xPATOL_INF_RESP = "";
            string vRESP_DOC = "";
            int Contador = 0;
            string VResp = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT * FROM GRAL_CONCEPTOS WITH(NOLOCK) WHERE CODCONC = '" + xCONCEPTO + "'";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            xPATOL_INF_RESP = dt.Rows[i]["PATOL_INF_RESP"].ToString();
                        }

                    }
                    catch (Exception e)
                    {
                        xPATOL_INF_RESP = "";
                    }
                }

                sSql = "SELECT CAB_DOCUMENTACION.CODDOC, CAB_DOCUMENTACION.RESP_DOC, DESCDOC, DOCASOC, FECRE ";
                sSql = sSql + " FROM CAB_DOCUMENTACION WITH(NOLOCK) INNER JOIN GRAL_DOCUMENTACION WITH(NOLOCK)  ";
                sSql = sSql + " ON CAB_DOCUMENTACION.CODDOC = GRAL_DOCUMENTACION.CODDOC  ";
                sSql = sSql + " WHERE DOCASOC='S' AND IDCAB = " + xIDCABECERA;

                using (SqlCommand cmd2 = new SqlCommand(sSql, con))
                {
                    DataTable dt2 = new DataTable();
                    try
                    {

                        SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                        da2.Fill(dt2);

                        Dadm = "[";
                        Contador = dt2.Rows.Count;
                        if (dt2.Rows.Count > 0)
                        {
                            for (i = 0; i < dt2.Rows.Count; i++)
                            { 
                                    vRESP_DOC = dt2.Rows[i]["RESP_DOC"].ToString();
                                    switch (vRESP_DOC)
                                    {
                                        case "N":
                                            VResp = "No Aplica";
                                            break;
                                        case "A":
                                            VResp = "Afiliado";
                                            break;
                                        case "P":
                                            VResp = "Prestador";
                                            break;
                                        default:
                                            VResp = "No Aplica";
                                            break;
                                    }  
                              
                                Dadm = Dadm + "{";
                                Dadm = Dadm + '"' + "Responsable" + '"' + ":" + '"' + VResp + '"' + ",";
                                Dadm = Dadm + '"' + "DOCASOC" + '"' + ":" + '"' + dt2.Rows[i]["DOCASOC"].ToString() + '"' + ",";
                                Dadm = Dadm + '"' + "Documento" + '"' + ":" + '"' + dt2.Rows[i]["DESCDOC"].ToString() + '"' + ",";
                                Dadm = Dadm + '"' + "IDDOC" + '"' + ":" + '"' + dt2.Rows[i]["CODDOC"].ToString() + '"' + ",";
                                Dadm = Dadm + '"' + "Fecha" + '"' + ":" + '"' + FormatoFecha2(dt2.Rows[i]["FECRE"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                Dadm = Dadm + '"' + "Resp" + '"' + ":" + '"' + vRESP_DOC + '"';

                                if (i < Contador - 1)
                                {
                                    Dadm = Dadm + "},";
                                }
                                else
                                {
                                    Dadm = Dadm + "}";
                                };
                            }
                            Dadm = Dadm + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            Dadm = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        Dadm = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            Dadm = Dadm + "{";
                            Dadm = Dadm + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                Dadm = Dadm + "}";
                            }
                            else
                            {
                                Dadm = Dadm + "},";
                            };
                        }
                        Dadm = Dadm + "]";
                    }
                    con.Close();
                    return Dadm;

                }
            }
        }

        public string DocumentosAdmN(string xIDCABECERA, string xCONCEPTO, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            int i = 0;
            string Dadm = "";
            string xPATOL_INF_RESP = "";
            string vRESP_DOC = "N";
            int Contador = 0;
            string VResp = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT * FROM GRAL_CONCEPTOS WITH(NOLOCK) WHERE CODCONC = '" + xCONCEPTO + "'";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            xPATOL_INF_RESP = dt.Rows[i]["PATOL_INF_RESP"].ToString();
                        }

                    }
                    catch (Exception e)
                    {
                        xPATOL_INF_RESP = "";
                    }
                }

                sSql = "SELECT CAB_DOCUMENTACION.CODDOC, CAB_DOCUMENTACION.RESP_DOC, DESCDOC, DOCASOC, FECRE ";
                sSql = sSql + " FROM CAB_DOCUMENTACION WITH(NOLOCK) INNER JOIN GRAL_DOCUMENTACION WITH(NOLOCK)  ";
                sSql = sSql + " ON CAB_DOCUMENTACION.CODDOC = GRAL_DOCUMENTACION.CODDOC  ";
                sSql = sSql + " WHERE DOCASOC='N' AND IDCAB = " + xIDCABECERA;

                using (SqlCommand cmd2 = new SqlCommand(sSql, con))
                {
                    DataTable dt2 = new DataTable();
                    try
                    {

                        SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                        da2.Fill(dt2);

                        Dadm = "[";
                        Contador = dt2.Rows.Count;
                        if (dt2.Rows.Count > 0)
                        {
                            for (i = 0; i < dt2.Rows.Count; i++)
                            {
                                vRESP_DOC = dt2.Rows[i]["RESP_DOC"].ToString();
                                switch (vRESP_DOC)
                                {
                                    case "N":
                                        VResp = "No Aplica";
                                        break;
                                    case "A":
                                        VResp = "Afiliado";
                                        break;
                                    case "P":
                                        VResp = "Prestador";
                                        break;
                                    default:
                                        VResp = "No Aplica";
                                        break;
                                }  

                                Dadm = Dadm + "{";
                                Dadm = Dadm + '"' + "Responsable" + '"' + ":" + '"' + VResp + '"' + ",";
                                Dadm = Dadm + '"' + "DOCASOC" + '"' + ":" + '"' + dt2.Rows[i]["DOCASOC"].ToString() + '"' + ",";
                                Dadm = Dadm + '"' + "Documento" + '"' + ":" + '"' + dt2.Rows[i]["DESCDOC"].ToString() + '"' + ",";
                                Dadm = Dadm + '"' + "IDDOC" + '"' + ":" + '"' + dt2.Rows[i]["CODDOC"].ToString() + '"' + ",";
                                Dadm = Dadm + '"' + "Fecha" + '"' + ":" + '"' + FormatoFecha2(dt2.Rows[i]["FECRE"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                Dadm = Dadm + '"' + "Resp" + '"' + ":" + '"' + vRESP_DOC + '"';

                                if (i < Contador - 1)
                                {
                                    Dadm = Dadm + "},";
                                }
                                else
                                {
                                    Dadm = Dadm + "}";
                                }
                            }
                            Dadm = Dadm + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            Dadm = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        Dadm = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            Dadm = Dadm + "{";
                            Dadm = Dadm + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                Dadm = Dadm + "}";
                            }
                            else
                            {
                                Dadm = Dadm + "},";
                            }
                        }
                        Dadm = Dadm + "]";
                    }
                    con.Close();
                    return Dadm;
                }
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

        string Subgrillas.agregarDocumento(string xIDCABECERA, string xCONCEPTO, string[] xIDDOC, string[] xRESP_DOC, string xDOCASOC, string[] xChecked, string[] xFEPM, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xfechaPM = "";
            SqlTransaction transaction;

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return "Su petición no puede realizarse.";
            }

            for (int i = 0; i < xIDDOC.Length; i++)
            {
                if (EsStrNuloBlanco(xIDCABECERA) == true
                  || EsStrNuloBlanco(xIDDOC[i].ToString()) == true
                  || EsStrNuloBlanco(xRESP_DOC[i].ToString()) == true
                  || EsStrNuloBlanco(xDOCASOC) == true
                  || (EsStrNuloBlanco(xFEPM[i].ToString()) == true && xIDDOC[i].ToString() == "PM")
                  || (xChecked[i].ToString() == "" || xChecked[i].ToString() == "NULL" || xChecked[i].ToString() == null)
                   )
                {
                    return "Su petición no puede realizarse.";
                }
 
                xFEPM[i] = FormatoFecha(xFEPM[i].ToString(), "yyyy/mm/dd", true);

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
                {
                    sSql = "UPDATE CAB_DOCUMENTACION  ";
                    sSql = sSql + "SET DOCASOC ='" + xDOCASOC + "',";
                    sSql = sSql + "USCRE ='" + xUserLog + "',";
                    sSql = sSql + "FECRE  ='" + DateTime.Now.ToString("yyyy/MM/dd") + "',";
                    sSql = sSql + "HORACRE ='" + DateTime.Now.ToString("HH:mm:ss") + "',";
                    sSql = sSql + "MODIMPORT =" + "CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END " + ",";

                    if (xRESP_DOC[i] == "N")
                    { 
                        sSql = sSql + "RESP_DOC = 'N'" + ","; }
                    else
                    {
                        if (xChecked[i].ToUpper() == "AFILIADO")
                        {
                            sSql = sSql + "RESP_DOC = 'A'";
                        }

                        else
                        {
                            sSql = sSql + "RESP_DOC = 'P'";
                        }
                    }

                    sSql = sSql + " WHERE IDCAB = '" + xIDCABECERA + "' AND CODDOC ='" + xIDDOC[i] + "'";
                    con.Open();
                    SqlCommand command = con.CreateCommand();
                    transaction = con.BeginTransaction("agregarDocumentoTransaction");

                    try
                    {
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = sSql;
                        command.ExecuteNonQuery();

                        if (command.ExecuteNonQuery() > 0) // Logro el Update
                        {
                            if (xIDDOC[i].ToString() == "PM")
                            {
                                if (xDOCASOC == "S")
                                { xfechaPM = xFEPM[i]; }
                                else
                                { xfechaPM = null; }

                                sSql = "UPDATE CABECERAS ";
                                if (xfechaPM == null)
                                {
                                    sSql = sSql + "SET FEPM= null,";
                                }
                                else
                                {
                                    sSql = sSql + "SET FEPM= '" + xfechaPM + "',";
                                }

                                sSql = sSql + "MODIMPORTADM = CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END ";
                                sSql = sSql + "WHERE ID = '" + xIDCABECERA + "'";

                                command.CommandText = sSql;
                            }
                            try
                            {
                                if (command.ExecuteNonQuery() > 0) // Logro el Update
                                {
                                    transaction.Commit();

                                }
                            }
                            catch (Exception ex)
                            {
                                con.Close();
                                return "Error al intentar realizar su peticion.";
                            }
                            //------------------------------------------------------------
                        }

                    }

                    catch (Exception ex)
                    {
                        transaction.Rollback("agregarDocumentoTransaction");
                        con.Close();
                        return "Error al intentar realizar su peticion.";
                    }

                    con.Close();
                }
            }

            return "";
        }
        string Subgrillas.responsableDocCheck(string xIDCABECERA, string xIDDOC, string Responsable, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            SqlTransaction transaction;

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xIDCABECERA) == true || EsStrNuloBlanco(xIDDOC) == true ||
                (Responsable == null && Responsable == ""))
            {
                xMensaje = "Su petición no puede realizarse.";
                return xMensaje;

            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "";
                sSql = sSql + "UPDATE CAB_DOCUMENTACION ";

                if (Responsable == "Afiliado")
                {
                    sSql = sSql + "SET RESP_DOC = 'P'";
                }
                else if (Responsable == "Prestador")
                {
                    sSql = sSql + "SET RESP_DOC = 'A'";
                }
                else
                {
                return "Responsable incorrecto.";
                }

                sSql = sSql + " WHERE IDCAB = '" + xIDCABECERA + "' AND CODDOC ='" + xIDDOC + "'";

                con.Open();
                SqlCommand command = con.CreateCommand();
                transaction = con.BeginTransaction("responsableDocCheckTransaction");

                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;
                    command.CommandText = sSql;

                    if (command.ExecuteNonQuery() > 0) 
                    {
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback("responsableDocCheckTransaction");
                    xMensaje = "Error al intentar intentar modificar el registro.";
                }

                con.Close();
                return xMensaje;
            }
        }

        private string FormatoFecha(string xDate, String xFormat, bool xTime)
        {
            string xDia = "";
            string xMes = "";
            string xAnio = "";

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

        private string FormatoHora(string xTime, String xFormat, bool xFtime)
        {
            string xHora = "";
            string xMinuto = "";
            string xSegundo = "";

            if (xTime == null || xTime == "" || xTime == "01/01/1900 00:00:00" || xTime == "01-01-1900 00:00:00" || xTime == "1900-01-01 00:00:00.000")
            {
                //Hora en blanco o null, retorno como viene...
                return xTime;
            }

            string MyDate = xTime;
            DateTime date = Convert.ToDateTime(MyDate);

            int Hora = date.Hour;
            int Minuto = date.Minute;
            int Segundo = date.Second;

            xHora = Hora.ToString();
            xMinuto = Minuto.ToString();
            xSegundo = Segundo.ToString();

            switch (xFormat)
            {
                case "hh:mm:ss":
                    xTime = xHora + ":" + xMinuto + ":" + xSegundo;
                    break;
                //default:
                //    Console.WriteLine("Default case");
                //    break;            }
            }
            if (xFtime == true)
            {
                //Fecha en blanco o null, retorno como viene...
                xTime = "1900-01-01 " + xTime;
            }
            return xTime;
        }
        string ValidarDatosConexion(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string xMensaje = "El/los campo/s ";

            if (EsStrNuloBlanco(xModulo) == true)
            {
                xMensaje = xMensaje + " Módulo,";
            }

            if (EsStrNuloBlanco(xBase) == true)
            {
                xMensaje = xMensaje + " Base,";
            }

            if (EsStrNuloBlanco(xUserLog) == true)
            {
                xMensaje = xMensaje + " Usuario de Login,";
            }

            if (EsStrNuloBlanco(xFilialLog) == true)
            {
                xMensaje = xMensaje + " Filial de Login";
            }

            //Valido longitud del mensaje...
            if (xMensaje.Length > 15)
            {
                xMensaje = xMensaje + " deben ser informados.";
                return xMensaje;
            }
            return "";
        }
        //------------------------------------------------------------------------------------------------------------------
        string Subgrillas.AddPresupuestoCont(string xIDCabecera, string xCodMedic, string xConcepto, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();

            string Add_Pres_Cont = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "";
                sSql = sSql + "INSERT INTO CAB_PRESUPUESTOS   ";
                sSql = sSql + " VALUES ('" + xIDCabecera + "',";
                sSql = sSql + "" + xCodMedic + ",";
                sSql = sSql + "'" + xUserLog + "',";
                sSql = sSql + "'" + DateTime.Now.ToString("yyyy/MM/dd") + "',";
                sSql = sSql + "'" + DateTime.Now.ToString("HH:mm:ss") + "',";
                sSql = sSql + "" + "NULL" + ",";
                sSql = sSql + "" + "NULL" + ",";
                sSql = sSql + "" + "NULL" + ",";
                sSql = sSql + "" + "NULL" + ",";
                sSql = sSql + "'" + "NO" + "',";
                sSql = sSql + "'" + "NO" + "')";

                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("Add_Pres_Cont");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    //-------------------------------------

                    Add_Pres_Cont = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                    //}
                    // Commiteo uno o los dos INSERTs
                    transaction.Commit();

                    //Invoco al metodo MedicamentosContPendyObt 
                    String a = this.MedicamentosContPendyObt(xIDCabecera, xConcepto, xModulo, xBase, xUserLog, xFilialLog);
                    xMensaje = a;
                    Add_Pres_Cont = a;
                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("Add_Pres_Cont");
                    Add_Pres_Cont = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return Add_Pres_Cont;
            }
        }

        string Subgrillas.RemPresupuestoCont(string xIDCabecera, string xCodMedic, string xConcepto, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();

            string Upd_Pres_Cont = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (EsValNumerico(xIDCabecera) == false)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su petición no puede realizarse." + '"' + "}]";
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "";
                sSql = sSql + "UPDATE CAB_PRESUPUESTOS   ";
                sSql = sSql + " SET ESTADO='BAJA',";
                sSql = sSql + " USMOD='" + xUserLog + "',";
                sSql = sSql + " FEMOD='" + DateTime.Now.ToString("yyyy/MM/dd") + "',";
                sSql = sSql + " HORAMOD='" + DateTime.Now.ToString("HH:mm:ss") + "'";
                sSql = sSql + " WHERE IDCAB = '" + xIDCabecera + "' AND CODMEDIC='" + xCodMedic + "' AND ESTADO IS NULL";

                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("Upd_Pres_Cont");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    //-------------------------------------

                    Upd_Pres_Cont = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                    //}
                    // Commiteo el UPDATE
                    transaction.Commit();

                    //Invoco al metodo MedicamentosContPendyObt 
                    String a = this.MedicamentosContPendyObt(xIDCabecera, xConcepto, xModulo, xBase, xUserLog, xFilialLog);
                    xMensaje = a;
                    Upd_Pres_Cont = a;
                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("Upd_Pres_Cont");
                    Upd_Pres_Cont = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return Upd_Pres_Cont;
            }
        }

        string Subgrillas.DatosCreayModCont(string xIDReg, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            //int i=0;
            string DcmC = "";
            string xFlag = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (EsValNumerico(xIDReg) == false)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su petición no puede realizarse." + '"' + "}]";
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT USCRE,FECRE,HORACRE,USMOD,FEMOD,HORAMOD ";
                sSql = sSql + " FROM RENGLONES WITH(NOLOCK)WHERE ID = " + xIDReg;


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
                            DcmC = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                if (string.IsNullOrEmpty(dt.Rows[i]["USMOD"].ToString()) == true)
                                {

                                    DcmC = DcmC + "{";
                                    DcmC = DcmC + '"' + "Concepto" + '"' + ":" + '"' + "Creación" + '"' + ",";
                                    DcmC = DcmC + '"' + "Detalle" + '"' + ":" + '"' + "EL DIA " + FormatoFecha2(dt.Rows[i]["FECRE"].ToString(), "dd/mm/yyyy", false) + " " + dt.Rows[i]["HORACRE"].ToString().Substring(11, 8) + " POR " + dt.Rows[i]["USCRE"].ToString() + '"';

                                }
                                else
                                {

                                    DcmC = DcmC + "{";
                                    DcmC = DcmC + '"' + "Concepto" + '"' + ":" + '"' + "Creación" + '"' + ",";
                                    DcmC = DcmC + '"' + "Detalle" + '"' + ":" + '"' + "EL DIA " + FormatoFecha2(dt.Rows[i]["FECRE"].ToString(), "dd/mm/yyyy", false) + " " + dt.Rows[i]["HORACRE"].ToString().Substring(11, 8) + " POR " + dt.Rows[i]["USCRE"].ToString() + '"' + ",";
                                    DcmC = DcmC + '"' + "Concepto" + '"' + ":" + '"' + "Modificación" + '"' + ",";
                                    DcmC = DcmC + '"' + "Detalle" + '"' + ":" + '"' + "EL DIA " + FormatoFecha2(dt.Rows[i]["feMod"].ToString(), "dd/mm/yyyy", false) + " " + dt.Rows[i]["HORAMOD"].ToString().Substring(11, 8) + " POR " + dt.Rows[i]["USMOD"].ToString() + '"';

                                }
                                if (i < Contador - 1)
                                {
                                    DcmC = DcmC + "},";
                                }
                                else
                                {
                                    DcmC = DcmC + "}";
                                };
                            }
                            DcmC = DcmC + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            DcmC = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        DcmC = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            DcmC = DcmC + "{";
                            DcmC = DcmC + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                DcmC = DcmC + "}";
                            }
                            else
                            {
                                DcmC = DcmC + "},";
                            };
                        }
                        DcmC = DcmC + "]";
                    }
                    con.Close();
                    return DcmC;

                }
            }
        }


        string Subgrillas.VisualizarLogCons(string xIDReg, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            //int i=0;
            string VlgC = "";
            string xFlag = "";
            string xfilAfil = "";
            string xnroAfil = "";
            string xBeneAfil = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (EsValNumerico(xIDReg) == false)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su petición no puede realizarse." + '"' + "}]";
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT * FROM Log_Cbios_Renglones WITH(NOLOCK) ";
                sSql = sSql + " WHERE IDRENG = " + xIDReg;
                sSql = sSql + " Order by Femod";

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
                            VlgC = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                // ALFA1
                                xfilAfil = dt.Rows[i]["filafil"].ToString().PadLeft(2, '0');
                                xnroAfil = dt.Rows[i]["nroafil"].ToString().PadLeft(7, '0');
                                xBeneAfil = dt.Rows[i]["benefafil"].ToString().PadLeft(2, '0');

                                //{["FeMod":"[feMod]","ID":"[IDRENG]","IDCabecera":"[IDCAB]","NroAfiliado":"[filAfil]&[nroAfil]&[benefAfil]",
                                //"Concepto":"[codConc]","FechaPrest":"[fePrest]","FechaFac":"[FECHAFC]","NroFC":"[nroFc]",
                                //"FechaRC":"[fechaRc]","NroRC":"[nroRc]","FechaRm":"[feRm]","NroRM":"[v]",//"FechaEgreso":"[FECBTEEGRESO]","NroEgreso":"[NROCBTEEGRESO]",
                                //"CodPrestador":"[codprestador]","Prestador":"[NOMPRESTADOR]","Usuario":"[Usuario]"]}

                                VlgC = VlgC + "{";
                                VlgC = VlgC + '"' + "FeMod" + '"' + ":" + '"' + "EL DIA " + FormatoFecha2(dt.Rows[i]["feMod"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                VlgC = VlgC + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["IDRENG"].ToString() + '"' + ",";
                                VlgC = VlgC + '"' + "IDCabecera" + '"' + ":" + '"' + dt.Rows[i]["IDCAB"].ToString() + '"' + ",";
                                VlgC = VlgC + '"' + "NroAfiliado" + '"' + ":" + '"' + xfilAfil + xnroAfil + xBeneAfil + '"' + ",";
                                VlgC = VlgC + '"' + "Concepto" + '"' + ":" + '"' + dt.Rows[i]["codConc"].ToString() + '"' + ",";
                                VlgC = VlgC + '"' + "FechaPrest" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[i]["fePrest"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                VlgC = VlgC + '"' + "FechaFac" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[i]["FECHAFC"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                VlgC = VlgC + '"' + "NroFC" + '"' + ":" + '"' + dt.Rows[i]["NroFC"].ToString() + '"' + ",";
                                VlgC = VlgC + '"' + "FechaRC" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[i]["fechaRc"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                VlgC = VlgC + '"' + "FechaRm" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[i]["feRm"].ToString(), "dd/mm/yyyy", false) + '"' + ",";

                                VlgC = VlgC + '"' + "NroRM" + '"' + ":" + '"' + dt.Rows[i]["nrorm"].ToString() + '"' + ",";
                                VlgC = VlgC + '"' + "FechaEgreso" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[i]["FECBTEEGRESO"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                VlgC = VlgC + '"' + "NroEgreso" + '"' + ":" + '"' + dt.Rows[i]["NROCBTEEGRESO"].ToString() + '"' + ",";
                                VlgC = VlgC + '"' + "CodPrestador" + '"' + ":" + '"' + dt.Rows[i]["CodPrestador"].ToString() + '"' + ",";
                                VlgC = VlgC + '"' + "Prestador" + '"' + ":" + '"' + dt.Rows[i]["NOMPRESTADOR"].ToString() + '"' + ",";
                                VlgC = VlgC + '"' + "Usuario" + '"' + ":" + '"' + dt.Rows[i]["Usuario"].ToString() + '"';


                                if (i < Contador - 1)
                                {
                                    VlgC = VlgC + "},";
                                }
                                else
                                {
                                    VlgC = VlgC + "}";
                                };
                            }
                            VlgC = VlgC + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            VlgC = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        VlgC = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            VlgC = VlgC + "{";
                            VlgC = VlgC + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                VlgC = VlgC + "}";
                            }
                            else
                            {
                                VlgC = VlgC + "},";
                            };
                        }
                        VlgC = VlgC + "]";
                    }
                    con.Close();
                    return VlgC;

                }
            }
        }

        private bool EsValNumerico(string xValor)
        {
            double number1 = 0;
            bool canConvert = double.TryParse(xValor, out number1);
            if (canConvert == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        string Subgrillas.EliminarDocumentoCont(string xIDReg, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlTransaction transaction;

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (EsValNumerico(xIDReg) == false)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su petición no puede realizarse." + '"' + "}]";
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = " INSERT INTO RENGLONESELIM SELECT * FROM RENGLONES  ";
                sSql = sSql + " WHERE ID =" + xIDReg;

                con.Open();

                SqlCommand command = con.CreateCommand();

                transaction = con.BeginTransaction("eliminarDocumentosCont");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;

                    if (command.ExecuteNonQuery() >= 0) // Logro el Insert
                    {
                        sSql = "DELETE FROM RENGLONES   WHERE ID = " + xIDReg;
                        command.CommandText = sSql;

                        try
                        {
                            if (command.ExecuteNonQuery() >= 0) // Logro el Delete
                            {// ALFA1

                                string sSql2 = "UPDATE RENGLONESELIM ";
                                sSql2 = sSql2 + "SET FECRE ='" + DateTime.Now.ToString("yyyy/MM/dd") + "', USCRE ='" + xUserLog + "', HORACRE='" + DateTime.Now.ToString("HH:mm:ss") + "', ";
                                sSql2 = sSql2 + "IMPORTADO ='NO', LIBROEGRESO = CASE WHEN LIBROEGRESO IS NULL then NULL else LIBROEGRESO + 'N' END ";
                                sSql2 = sSql2 + "WHERE ID =" + xIDReg;
                                command.CommandText = sSql2;
                                try
                                {

                                    if (command.ExecuteNonQuery() >= 0) // Logro el Update
                                    {
                                        sSql = "INSERT INTO CONTABILIDAD  ";
                                        sSql = sSql + "SELECT IDCAB, IDRENG, CODOS, TIPORDO, 'PEND' AS NROASTO,";
                                        sSql = sSql + "NULL AS FEASTO, FILDEBITO, IMPORTE*-1 AS IMPORTE,'P' AS TIPOASTO, ";
                                        sSql = sSql + "IMPORTECRED *-1 AS IMPORTECRED , IMPORTEPREV*-1 AS IMPORTEPREV,";
                                        sSql = sSql + "IMPORTERDO *-1 AS IMPORTERDO,'SI' AS IMPORTADO,NULL AS ASTOSAP, ";
                                        sSql = sSql + xFilialLog + " AS FILORIGEN,CAP,PLAN_AFIL FROM CONTABILIDAD  WITH(NOLOCK) ";
                                        sSql = sSql + "WHERE IDRENG =" + xIDReg;
                                        command.CommandText = sSql;
                                        try
                                        {


                                            if (command.ExecuteNonQuery() >= 0) // Logro el Insert
                                            {

                                                sSql = "DELETE FROM LOG_CBIOS_RENGLONES";
                                                sSql = sSql + " WHERE IDRENG = " + xIDReg;
                                                command.CommandText = sSql;
                                                try
                                                {
                                                    if (command.ExecuteNonQuery() >= 0) // Logro el Delete
                                                    {
                                                        transaction.Commit();
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + "}]";
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + "}]";
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + "}]";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + "}]";
                        }
                        //------------------------------------------------------------
                    }

                }

                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("eliminarDocumentosCont");
                    xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + "}]";
                }

                con.Close();
            }

            return xMensaje;
        }

        public string CargarFormModif(long xIDReg, string xConcepto, string xTipo, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();

            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            string cFrmMod = "";
            float VTOTAL = 0;
            int Signo = 0;
            string xNroAfiliado = "";
            string xNroAfiliado2 = "";
            string xNroFactura = "";
            string ImporteUN = "";
            float ImporteTotal = 0;
            string vNroPrestador = "";
            string vNomPrestador = "";
            string vNroRM = "";
            string IdCabecera = "";
            int ContadorPrincipal = 0;
            bool cmdAceptar = false;
            float xImporteTotal = 0;
            string vImporte = "0";
            string xCodConcSelect1 = "";

            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);

            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (ExisteCodCon(xConcepto, xModulo, xBase, xUserLog, xFilialLog) == false)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su petición no puede realizarse." + '"' + "}]";
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = sSql + "SELECT RENGLONES.*,GRAL_AFILIADOS.NOMBRE +' '+ GRAL_AFILIADOS.APELLIDO AS NombreAFIL,(Select GRAL_AFILIADOS.NOMBRE +' '+ GRAL_AFILIADOS.APELLIDO ";
                sSql = sSql + "FROM GRAL_AFILIADOS WHERE NROAFIL = RENGLONES.NROAFIL2 AND ";
                sSql = sSql + "FILAFIL = RENGLONES.FILAFIL2 AND BENEFAFIL = RENGLONES.BENEFAFIL2) "; 
                sSql = sSql + "AS NombreAFIL2 ";
                sSql = sSql + "FROM RENGLONES WITH(NOLOCK) ";
                sSql = sSql + "LEFT OUTER JOIN GRAL_AFILIADOS "; 
                sSql = sSql + "ON RENGLONES.FILAFIL = GRAL_AFILIADOS.FILAFIL AND "; 
                sSql = sSql + "RENGLONES.NROAFIL = GRAL_AFILIADOS.NROAFIL AND ";
                sSql = sSql + "RENGLONES.BENEFAFIL = GRAL_AFILIADOS.BENEFAFIL ";
                sSql = sSql + "WHERE ID = " + xIDReg;

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        cFrmMod = "[";

                        ContadorPrincipal = dt.Rows.Count;

                        try { 
                            if (dt.Rows.Count > 0)
                            {
                                cFrmMod = cFrmMod + "{";

                                    if (float.Parse(dt.Rows[0]["Importe"].ToString()) < 0)
                                    { Signo = -1; }
                                    else
                                    { Signo = 1; }

                                    if (dt.Rows[0]["UBIC"].ToString() != xFilialLog)
                                    { cmdAceptar = false; }
                                    else
                                    { cmdAceptar = true; }

                                    xNroAfiliado = dt.Rows[0]["filafil"].ToString().PadLeft(2, '0') + dt.Rows[0]["nroafil"].ToString().PadLeft(7, '0') + dt.Rows[0]["benefafil"].ToString().PadLeft(2, '0');
                                    xNroAfiliado2 = dt.Rows[0]["filafil2"].ToString().PadLeft(2, '0') + dt.Rows[0]["nroafil2"].ToString().PadLeft(7, '0') + dt.Rows[0]["benefafil2"].ToString().PadLeft(2, '0');
                                    xNroFactura = dt.Rows[0]["nroFc"].ToString();
                                    xNroFactura = xNroFactura.Substring(0, 1) + xNroFactura.Substring(1, 4) + xNroFactura.Substring(5, 8);
                                    vImporte = dt.Rows[0]["Importe"].ToString();

                                    string vImportePres = dt.Rows[0]["ImportePrest"].ToString();

                                    if (String.IsNullOrEmpty(dt.Rows[0]["codprestador"].ToString()) == false)
                                    {
                                        vNroPrestador = dt.Rows[0]["codprestador"].ToString();
                                        vNomPrestador = dt.Rows[0]["NOMPRESTADOR"].ToString();
                                    }
                                    else
                                    {
                                        vNroPrestador = "";
                                        vNomPrestador = "";
                                    }
                                
                                    vNroRM = dt.Rows[0]["nrorm"].ToString();
                                    IdCabecera = dt.Rows[0]["IDCAB"].ToString();

                                    cFrmMod = cFrmMod + '"' + "btoAceptar" + '"' + ":" + '"' + cmdAceptar + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "idCabecera" + '"' + ":" + '"' + dt.Rows[0]["IDCAB"].ToString() + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NroAfiliado" + '"' + ":" + '"' + xNroAfiliado + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NroAfiliado2" + '"' + ":" + '"' + xNroAfiliado2 + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NombreAfiliado" + '"' + ":" + '"' + dt.Rows[0]["NombreAFIL"].ToString() + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NombreAfiliado2" + '"' + ":" + '"' + dt.Rows[0]["NombreAFIL2"].ToString() + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NroEgreso" + '"' + ":" + '"' + dt.Rows[0]["NROCBTEEGRESO"].ToString() + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "Concepto" + '"' + ":" + '"' + xConcepto + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "FeEgreso" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[0]["FECBTEEGRESO"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NroCheque" + '"' + ":" + '"' + dt.Rows[0]["NROCHEQUE"].ToString() + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "Banco" + '"' + ":" + '"' + dt.Rows[0]["Banco"].ToString() + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NroCuenta" + '"' + ":" + '"' + dt.Rows[0]["NroCuenta"].ToString() + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NroFact" + '"' + ":" + '"' + xNroFactura + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "FechaFact" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[0]["FECHAFC"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "Importe" + '"' + ":" + '"' + vImporte + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "ImportePres" + '"' + ":" + '"' + vImportePres + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "CodPrestador" + '"' + ":" + '"' + vNroPrestador + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NombrePrest" + '"' + ":" + '"' + vNomPrestador + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NroRM" + '"' + ":" + '"' + vNroRM + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "FechaRM" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[0]["feRm"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "FePrestacion" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[0]["fePrest"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "Practica" + '"' + ":" + '"' + dt.Rows[0]["ORDENPRACTICA"].ToString() + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "Tramite" + '"' + ":" + '"' + dt.Rows[0]["NROTRAMITE"].ToString() + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "FeCierre" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[0]["FECIERRE"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "NroRC" + '"' + ":" + '"' + dt.Rows[0]["NroRC"].ToString() + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "FeRC" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[0]["FeRC"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    cFrmMod = cFrmMod + '"' + "FeDebito" + '"' + ":" + '"' + FormatoFecha2(dt.Rows[0]["FEDEBITO"].ToString(), "dd/mm/yyyy", false) + '"' ;
                                    xCodConcSelect1 = dt.Rows[0]["codConc"].ToString();
                            }
                            else
                            {
                                return "No se puede continuar con el proceso dado que los datos no son validos.";
                            }
                        }
                        catch (Exception e)
                        {
                            return "Error al procesar los datos: " + e.Message;
                        }
                    }
                    catch (SqlException ex)
                    {
                        return "Error de Base de datos: " + ex.Message;
                    }

                    // Select 2 ---------------------------------------------------------------------------
                    if (xCodConcSelect1 == "DISCAPACIDAD")
                    {
                        sSql = "SELECT DISTINCT CODESP, CODMOD ";
                        sSql = sSql + "FROM CAB_DOCUMENTACION_PRESUP WITH(NOLOCK) ";
                        sSql = sSql + "WHERE IDCAB= " + IdCabecera + " AND CUIT= " + vNroPrestador + " ";
                        sSql = sSql + "AND CODESP IS NOT NULL AND CODMOD IS NOT NULL";

                        using (SqlCommand cmd2 = new SqlCommand(sSql, con))
                        {
                            DataTable dt2 = new DataTable();
                            try
                            {
                                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                                da2.Fill(dt2);
                                try
                                {
                                    if (dt2.Rows.Count > 0)
                                    {
                                        cFrmMod = cFrmMod + "," + '"' + "modEspObj" + '"' + ":" + "[";
                                        for (int i2 = 0; i2 < dt2.Rows.Count; i2++)
                                        {
                                            cFrmMod = cFrmMod + "{";
                                            cFrmMod = cFrmMod + '"' + "ModEsp" + '"' + ":" + '"' + dt2.Rows[i2]["CODMOD"].ToString() + "-" + dt2.Rows[i2]["CODESP"].ToString() + '"';
                                            cFrmMod = cFrmMod + "}";
                                            if (i2 < (dt2.Rows.Count - 1))
                                            {
                                                cFrmMod = cFrmMod + ",";
                                            }

                                        }
                                        cFrmMod = cFrmMod + "]";
                                    }
                                    else
                                    {
                                        cFrmMod = cFrmMod + "," + '"' + "modEspObj" + '"' + ":" + "[";
                                        cFrmMod = cFrmMod + "{" + '"' + "ModEsp" + '"' + ":" + '"' + "" + '"' + "}";
                                        cFrmMod = cFrmMod + "]";
                                    }
                                }
                                catch (Exception e)
                                {
                                    return "Error al procesar los datos: " + e.Message;
                                }
                            }
                            catch (SqlException ex)
                            {
                                return "Error de Base de datos: " + ex.Message;
                            }
                        }
                    }
                    else
                    {
                        cFrmMod = cFrmMod + "," + '"' + "modEspObj" + '"' + ":" + "[";
                        cFrmMod = cFrmMod + "{" + '"' + "ModEsp" + '"' + ":" + '"' + "" + '"' + "}";
                        cFrmMod = cFrmMod + "]";
                    }
                     
                            sSql = "SELECT NRORENG, CANT, RENG_DESCRIPCION.CODMEDIC AS CODMEDIC,DESCMEDIC,DOSIS, ";
                            sSql = sSql + "CANTUNID,IMPUNIT FROM RENG_DESCRIPCION  WITH(NOLOCK) ";
                            sSql = sSql + "INNER JOIN GRAL_MEDICAMENTOS  WITH(NOLOCK) ";
                            sSql = sSql + "ON RENG_DESCRIPCION.CODMEDIC = GRAL_MEDICAMENTOS.CODMEDIC ";
                            sSql = sSql + " WHERE IDRENG = '" + xIDReg + "' AND ESTADO IS NULL AND ";
                            sSql = sSql + "GRAL_MEDICAMENTOS.CODCONC ='" + xConcepto + "'";
                            sSql = sSql + " ORDER BY IDRENG";

                            using (SqlCommand cmd3 = new SqlCommand(sSql, con))
                            {
                                DataTable dt3 = new DataTable();
                                try
                                {
                                    SqlDataAdapter da3 = new SqlDataAdapter(cmd3);
                                    da3.Fill(dt3);

                                    int Contador3 = dt3.Rows.Count;
                                    try{
                                        if (dt3.Rows.Count > 0)
                                        {
                                            cFrmMod = cFrmMod +"," + '"' + "medicamentosObj" + '"' + ":" + "[";
                                            for (int i3 = 0; i3 < dt3.Rows.Count; i3++)
                                            {
                                                ImporteUN = dt3.Rows[i3]["IMPUNIT"].ToString();
                                                xImporteTotal = float.Parse(dt3.Rows[i3]["CANT"].ToString()) * Signo * float.Parse(dt3.Rows[i3]["IMPUNIT"].ToString());//Format(rs!CANT * rs!IMPUNIT * Signo, "#,##0.00")
                                             //   ImporteTotal = xImporteTotal;
                                                VTOTAL = VTOTAL + xImporteTotal;

                                                cFrmMod = cFrmMod + "{";
                                                cFrmMod = cFrmMod + '"' + "NroReng" + '"' + ":" + '"' + dt3.Rows[i3]["NRORENG"].ToString() + '"' + ",";
                                                cFrmMod = cFrmMod + '"' + "CANTMEDIC" + '"' + ":" + '"' + dt3.Rows[i3]["CANT"].ToString() + '"' + ",";
                                                cFrmMod = cFrmMod + '"' + "CodigoMed" + '"' + ":" + '"' + dt3.Rows[i3]["CODMEDIC"].ToString() + '"' + ",";
                                                cFrmMod = cFrmMod + '"' + "MedicamentoDesc" + '"' + ":" + '"' + dt3.Rows[i3]["DESCMEDIC"].ToString() + '"' + ",";
                                                cFrmMod = cFrmMod + '"' + "Dosis" + '"' + ":" + '"' + dt3.Rows[i3]["DOSIS"].ToString() + '"' + ",";
                                                cFrmMod = cFrmMod + '"' + "CantUn" + '"' + ":" + '"' + dt3.Rows[i3]["CANTUNID"].ToString() + '"' + ",";
                                                cFrmMod = cFrmMod + '"' + "Precio" + '"' + ":" + '"' + ImporteUN + '"' + ",";
                                                cFrmMod = cFrmMod + '"' + "PrecioTotal" + '"' + ":" + '"' + xImporteTotal + '"' + ",";
                                                cFrmMod = cFrmMod + '"' + "Tipo" + '"' + ":" + '"' + "MODIF" + '"';
                                                cFrmMod = cFrmMod + "}";

                                                if (i3 < (dt3.Rows.Count - 1))
                                                {
                                                    cFrmMod = cFrmMod + ",";
                                                }
                                            }
                                            cFrmMod = cFrmMod + "]";
                                        }
                                        else
                                        {
                                            cFrmMod = cFrmMod +"," + '"' + "medicamentosObj" + '"' + ":" + "[";
                                            cFrmMod = cFrmMod + "{}";
                                            cFrmMod = cFrmMod + "]";
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        return "Error al procesar los datos: " + e.Message;
                                    }
                                }
                                catch (SqlException ex)
                                {
                                    return "Error de Base de datos: " + ex.Message;
                                }
                            }
                        //        Select 4:
                                sSql = "SELECT * FROM RENG_RECIBOSANTICIPOS  WITH(NOLOCK) ";
                                sSql = sSql + " WHERE IDRENG = '" + xIDReg + "' AND ESTADO IS NULL ";
                                sSql = sSql + " ORDER BY NRORENG";

                                using (SqlCommand cmd4 = new SqlCommand(sSql, con))
                                {
                                    DataTable dt4 = new DataTable();
                                    try
                                    {
                                        SqlDataAdapter da4 = new SqlDataAdapter(cmd4);
                                        da4.Fill(dt4);
                                        try{
                                            if (dt4.Rows.Count > 0)
                                            {
                                                cFrmMod = cFrmMod +"," +'"' + "anticipoObj" + '"' + ":" + "[";

                                                for (int i4 = 0; i4 < dt4.Rows.Count; i4++)
                                                {
                                                    cFrmMod = cFrmMod + "{";
                                                    cFrmMod = cFrmMod + '"' + "AntNroReng" + '"' + ":" + '"' + dt4.Rows[i4]["NroReng"].ToString() + '"' + ",";
                                                    cFrmMod = cFrmMod + '"' + "AntNroRC" + '"' + ":" + '"' + dt4.Rows[i4]["nroRC"].ToString() + '"' + ",";
                                                    cFrmMod = cFrmMod + '"' + "Tipo" + '"' + ":" + '"' + "MODIF" + '"'+ "}";
                                                    if (i4 < (dt4.Rows.Count - 1))
                                                    {
                                                        cFrmMod = cFrmMod + ",";
                                                    }
                                                }
                                                cFrmMod = cFrmMod + "]";
                                            }
                                            else
                                            {
                                                cFrmMod = cFrmMod + "," + '"' + "anticipoObj" + '"' + ":" + "[";
                                                cFrmMod = cFrmMod + "{}";
                                                cFrmMod = cFrmMod + "]";
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            return "Error al procesar los datos: " + e.Message;
                                        }
                                    }
                                    catch (SqlException ex)
                                    {
                                        return "Error de Base de datos: " + ex.Message;
                                    }
                                }
                        //        Select 5: revisar el documento
                                long idCabeceraAux = 0;

                                try
                                {
                                     idCabeceraAux = long.Parse(IdCabecera.ToString());
                                }
                                catch (Exception e)
                                {
                                     idCabeceraAux = 0;
                                }

                                if (idCabeceraAux != 0)
                                {
                                    sSql = "SELECT GRAL_MEDICAMENTOS.DESCMEDIC AS DESCMEDIC, AVG(IMPUNIT) AS IMPORTEUNIT ";
                                    sSql = sSql + "FROM GRAL_MEDICAMENTOS WITH(NOLOCK) ";
                                    sSql = sSql + "INNER JOIN (RENG_DESCRIPCION WITH(NOLOCK) ";
                                    sSql = sSql + "INNER JOIN RENGLONES WITH(NOLOCK) ";
                                    sSql = sSql + "ON RENG_DESCRIPCION.IDRENG = RENGLONES.ID) ";
                                    sSql = sSql + "ON RENG_DESCRIPCION.CODMEDIC = GRAL_MEDICAMENTOS.CODMEDIC ";
                                    sSql = sSql + "WHERE IDCAB =" + IdCabecera + " AND ESTADO IS NULL ";
                                    sSql = sSql + "GROUP BY GRAL_MEDICAMENTOS.DESCMEDIC";
                                    
                                    using (SqlCommand cmd5 = new SqlCommand(sSql, con))
                                    {
                                        DataTable dt5 = new DataTable();
                                        try
                                        {
                                            SqlDataAdapter da5 = new SqlDataAdapter(cmd5);
                                            da5.Fill(dt5);

                                            try
                                            {
                                                if (dt5.Rows.Count > 0)
                                                {
                                                    cFrmMod = cFrmMod +","+ '"' + "medicamentos2Obj" + '"' + ":" + "[";

                                                    for (int i5 = 0; i5 < dt5.Rows.Count; i5++)
                                                    {
                                                        cFrmMod = cFrmMod + "{";
                                                        cFrmMod = cFrmMod + '"' + "Medicamento" + '"' + ":" + '"' + dt5.Rows[i5]["DESCMEDIC"].ToString() + '"' + ",";
                                                        cFrmMod = cFrmMod + '"' + "ImporteUn" + '"' + ":" + '"' + dt5.Rows[i5]["IMPORTEUNIT"].ToString() + '"' + "}";

                                                        if (i5 < (dt5.Rows.Count - 1))
                                                        {
                                                            cFrmMod = cFrmMod + ",";
                                                        }
                                                    }
                                                    cFrmMod = cFrmMod + "]";
                                                }
                                                else
                                                {
                                                    cFrmMod = cFrmMod + "," + '"' + "medicamentos2Obj" + '"' + ":" + "[";
                                                    cFrmMod = cFrmMod + "{}";
                                                    cFrmMod = cFrmMod + "]";
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                return "Error al procesar los datos: " + e.Message;
                                            }
                                        }
                                        catch (SqlException ex)
                                        {
                                            return "Error de Base de datos: " + ex.Message;
                                        }
                                  }                       
                               }

                                cFrmMod = cFrmMod + "," + '"' + "SaldoFC" + '"' + ":" + '"' + VTOTAL + '"';

                    cFrmMod = cFrmMod + "}]";
                    con.Close();
                    return cFrmMod;
                }
            }
        }

        //-------------------------------------------------------------------------------------
        bool ExisteCodCon(string xcodconc,string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            if (xcodconc == null)
            {
                return false;

            }
            int Cant1 = 0;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "SELECT isnull(COUNT(*),0) AS CANTIDAD1 ";
                sSQL = sSQL + " FROM GRAL_CONCEPTOS  WITH(NOLOCK) ";
                sSQL = sSQL + " WHERE  CODCONC ='" + xcodconc + "'";

                using (SqlCommand cmd = new SqlCommand(sSQL, con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        if (Int32.Parse(dt.Rows[0]["CANTIDAD1"].ToString()) > 0)
                        {
                            Cant1 = Int32.Parse(dt.Rows[0]["CANTIDAD1"].ToString());
                        }

                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        return false;
                    }

                    con.Close();
                    if (Cant1 == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

            }
        }
        // Funciones auxiliares a Instancias desde otras clases-------------------------------------------------------------
        public string MedicamentosContPendyObt(string xIDCabecera, string xConcepto, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string oMedCoPeObt = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xIDCabecera) == true ||
                EsValNumerico(xIDCabecera) == false)
            {
                return "Su petición no puede realizarse.";
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT RENG_DESCRIPCION.CODMEDIC, DESCMEDIC,'NULL' AS FECHA ";
                sSql = sSql + "FROM GRAL_MEDICAMENTOS WITH (NOLOCK) ";
                sSql = sSql + "INNER JOIN (RENG_DESCRIPCION WITH (NOLOCK) ";
                sSql = sSql + "INNER JOIN RENGLONES WITH (NOLOCK) ";
                sSql = sSql + "ON RENG_DESCRIPCION.IDRENG = RENGLONES.ID) ";
                sSql = sSql + "ON RENG_DESCRIPCION.CODMEDIC = GRAL_MEDICAMENTOS.CODMEDIC ";
                sSql = sSql + "WHERE IDCAB = '" + xIDCabecera + "' AND ESTADO IS NULL ";
                sSql = sSql + "GROUP BY RENG_DESCRIPCION.CODMEDIC, DESCMEDIC ";

                using (SqlCommand cmdsd = new SqlCommand(sSql, con))
                {
                    DataTable dtsd = new DataTable();
                    try 
                    {
                        SqlDataAdapter dasd = new SqlDataAdapter(cmdsd);
                        dasd.Fill(dtsd);
                    
                        if (dtsd.Rows.Count > 0)
                        {
                            sSql = "SELECT RENG_DESCRIPCION.CODMEDIC, DESCMEDIC, ";
                            sSql = sSql + "(SELECT Pres.FECRE FROM CAB_PRESUPUESTOS Pres WHERE Reng.IDCAB = Pres.IDCAB and G_Med.CODMEDIC = Pres.CODMEDIC AND Pres.ESTADO IS NULL) AS FECHA ";
                            sSql = sSql + "FROM GRAL_MEDICAMENTOS G_Med WITH (NOLOCK) ";
                            sSql = sSql + "INNER JOIN (RENG_DESCRIPCION WITH (NOLOCK) ";
                            sSql = sSql + "INNER JOIN RENGLONES Reng WITH (NOLOCK) ";
                            sSql = sSql + "ON RENG_DESCRIPCION.IDRENG = Reng.ID) ";
                            sSql = sSql + "ON RENG_DESCRIPCION.CODMEDIC = G_Med.CODMEDIC ";
                            sSql = sSql + "WHERE Reng.IDCAB = '" + xIDCabecera + "' AND ESTADO IS NULL ";
                            sSql = sSql + "GROUP BY RENG_DESCRIPCION.CODMEDIC, DESCMEDIC,Reng.IDCAB,G_Med.CODMEDIC";

                            using (SqlCommand cmds1 = new SqlCommand(sSql, con))
                            {
                                DataTable dts1 = new DataTable();
                                try
                                {
                                    SqlDataAdapter das1 = new SqlDataAdapter(cmds1);
                                    das1.Fill(dts1);

                                    oMedCoPeObt = "[";
                                    for (int i = 0; i < dts1.Rows.Count; i++)
                                    {
                                      oMedCoPeObt = oMedCoPeObt + "{";
                                      oMedCoPeObt = oMedCoPeObt + '"' + "Codigo" + '"' + ":" + '"' + dts1.Rows[i]["CODMEDIC"].ToString() + '"' + ",";
                                      oMedCoPeObt = oMedCoPeObt + '"' + "Descripcion" + '"' + ":" + '"' + dts1.Rows[i]["DESCMEDIC"].ToString() + '"' + ",";

                                      if (dts1.Rows[i]["FECHA"].ToString() == "" || dts1.Rows[i]["FECHA"].ToString() == null)
                                      {
                                          oMedCoPeObt = oMedCoPeObt + '"' + "Fecha" + '"' + ":" + '"' + "" + '"' + "}";
                                      }
                                      else
                                      {
                                          oMedCoPeObt = oMedCoPeObt + '"' + "Fecha" + '"' + ":" + '"' + FormatoFecha(dts1.Rows[i]["FECHA"].ToString(), "dd/mm/yyyy", false) + '"' + "}";
                                      }

                                      if (i < dts1.Rows.Count - 1)
                                      {
                                        oMedCoPeObt = oMedCoPeObt + ",";
                                      }else{
                                         oMedCoPeObt = oMedCoPeObt + "";
                                      }
                                    }
                                    oMedCoPeObt = oMedCoPeObt + "]"; 
                                }
                                catch (SqlException ex)
                                {
                                    return "Error en el servicio: " + ex.Message;
                                }
                            }
                        }
                        else
                        { 
                            sSql = "SELECT CODMEDIC,DESCMEDIC,'NULL'AS FECHA ";
                            sSql = sSql + "FROM GRAL_MEDICAMENTOS WITH (NOLOCK) INNER JOIN GRAL_CONCEPTOS ";
                            sSql = sSql + "WITH (NOLOCK) ON GRAL_MEDICAMENTOS.CODCONC = GRAL_CONCEPTOS.CODCONC ";
                            sSql = sSql + "WHERE GRAL_CONCEPTOS.CODCONC='" + xConcepto + "' ";
                            sSql = sSql + "AND GRAL_CONCEPTOS.TIPOCONC='PRESTACION' ";
                            sSql = sSql + "ORDER BY CODMEDIC";

                            using (SqlCommand cmdsd2 = new SqlCommand(sSql, con))
                            {
                                DataTable dtsd2 = new DataTable();
                                try 
                                {
                                    SqlDataAdapter dasd2 = new SqlDataAdapter(cmdsd2);
                                    dasd2.Fill(dtsd2);
 
                                    if (dtsd2.Rows.Count > 0)
                                    {
                                        sSql = "SELECT CODMEDIC,DESCMEDIC, ";
                                        sSql = sSql + "(SELECT Pres.FECRE FROM CAB_PRESUPUESTOS Pres WHERE G_Med.CODMEDIC = Pres.CODMEDIC AND Pres.ESTADO IS NULL) AS FECHA ";
                                        sSql = sSql + "FROM GRAL_MEDICAMENTOS G_Med WITH (NOLOCK) INNER JOIN GRAL_CONCEPTOS ";
                                        sSql = sSql + "WITH (NOLOCK) ON G_Med.CODCONC = GRAL_CONCEPTOS.CODCONC ";
                                        sSql = sSql + "WHERE GRAL_CONCEPTOS.CODCONC= '" + xConcepto + "' ";
                                        sSql = sSql + "AND GRAL_CONCEPTOS.TIPOCONC='PRESTACION' ";
                                        sSql = sSql + "ORDER BY CODMEDIC";

                                        using (SqlCommand cmds11 = new SqlCommand(sSql, con))
                                        {
                                            DataTable dts11 = new DataTable();
                                            try
                                            {
                                                SqlDataAdapter das2 = new SqlDataAdapter(cmds11);
                                                das2.Fill(dts11);

                                                    if (dts11.Rows.Count > 0)
                                                    {
                                                        oMedCoPeObt = "[";
                                                        for (int i = 0; i < dts11.Rows.Count; i++)
                                                        {
                                                           oMedCoPeObt = oMedCoPeObt + "{";
                                                           oMedCoPeObt = oMedCoPeObt + '"' + "Codigo" + '"' + ":" + '"' + dts11.Rows[i]["CODMEDIC"].ToString() + '"' + ",";
                                                           oMedCoPeObt = oMedCoPeObt + '"' + "Descripcion" + '"' + ":" + '"' + dts11.Rows[i]["DESCMEDIC"].ToString() + '"' + ",";

                                                           if (dts11.Rows[i]["FECHA"].ToString() == "" || dts11.Rows[i]["FECHA"].ToString() == null)
                                                           {
                                                               oMedCoPeObt = oMedCoPeObt + '"' + "Fecha" + '"' + ":" + '"' + "" + '"' + "}";
                                                           }
                                                           else
                                                           {
                                                               oMedCoPeObt = oMedCoPeObt + '"' + "Fecha" + '"' + ":" + '"' + FormatoFecha(dts11.Rows[i]["FECHA"].ToString(), "dd/mm/yyyy", false) + '"' + "}";
                                                           }

                                                           if (i < dts11.Rows.Count - 1)
                                                           {
                                                              oMedCoPeObt = oMedCoPeObt + ",";
                                                           }
                                                           else
                                                           {
                                                              oMedCoPeObt = oMedCoPeObt + "";
                                                           } 
                                                        } 
                                                        oMedCoPeObt = oMedCoPeObt + "]"; 
                                                    }
                                            } 
                                            catch (SqlException ex)
                                            {
                                                return "Error en el servicio: " + ex.Message;     
                                            }                                                
                                        }
                                    }else {
                                         oMedCoPeObt = "[]"; 
                                    }
                                } catch (SqlException ex){
                                    return "Error en el servicio: " + ex.Message;          
                                }
                              }
                        }
                    }
                    catch (SqlException ex)
                    {
                        return "Error en el servicio: " + ex.Message;
                    }
                }
                con.Close(); 
             }   
             return oMedCoPeObt;    
        }
         
       
        public string DocumentosContables(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string oDocCont = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xIDCabecera) == true ||
                EsValNumerico(xIDCabecera) == false
               )
            {
                xMensaje = "Su petición no puede realizarse.";
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT CAB_DOCUMENTACION.CODDOC, DESCDOC, DOCASOC, FECRE ";
                sSql = sSql + "FROM CAB_DOCUMENTACION WITH (NOLOCK) ";
                sSql = sSql + "INNER JOIN GRAL_DOCUMENTACION WITH (NOLOCK) ";
                sSql = sSql + "ON CAB_DOCUMENTACION.CODDOC = GRAL_DOCUMENTACION.CODDOC  ";
                sSql = sSql + "WHERE IDCAB = '" + xIDCabecera + "' ";

                using (SqlCommand cmd2 = new SqlCommand(sSql, con))
                {
                    DataTable dt2 = new DataTable();
                    try
                    {
                        SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                        da2.Fill(dt2);

                        int Contador = dt2.Rows.Count;
                        if (dt2.Rows.Count > 0)
                        {
                            oDocCont = "[";
                            for (int i = 0; i < dt2.Rows.Count; i++)
                            {

                                oDocCont = oDocCont + "{";
                                oDocCont = oDocCont + '"' + "Codigo" + '"' + ":" + '"' + dt2.Rows[i]["CODDOC"].ToString() + '"' + ",";
                                oDocCont = oDocCont + '"' + "Descripcion" + '"' + ":" + '"' + dt2.Rows[i]["DESCDOC"].ToString() + '"' + ",";
                                oDocCont = oDocCont + '"' + "DocAsociado" + '"' + ":" + '"' + dt2.Rows[i]["DOCASOC"].ToString() + '"' + ",";
                                oDocCont = oDocCont + '"' + "Fecha" + '"' + ":" + '"' + FormatoFecha(dt2.Rows[i]["FECRE"].ToString(), "dd/mm/yyyy", false) + '"';


                                if (i < Contador - 1)
                                {
                                    oDocCont = oDocCont + "},";
                                }
                                else
                                {
                                    oDocCont = oDocCont + "}";
                                };

                            }
                            oDocCont = oDocCont + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "DocumentosContables:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            oDocCont = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        oDocCont = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            oDocCont = oDocCont + "{";
                            oDocCont = oDocCont + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                oDocCont = oDocCont + "}";
                            }
                            else
                            {
                                oDocCont = oDocCont + "},";
                            };
                        }
                        oDocCont = oDocCont + "]";
                    }
                    con.Close();
                    return oDocCont;

                }
            }
        }
        public string DocContable(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string oDocPend = "";
            int bandera = 0;

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xIDCabecera) == true ||
                EsValNumerico(xIDCabecera) == false
               )
            {
                return "Su petición no puede realizarse.";
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT ID,ORDENPRACTICA, IMPORTE, FEPREST, NROTRAMITE, FECIERRE, ";
                sSql = sSql + "NOMPRESTADOR,  NROFC, FECHAFC, NRORC, FERC,FECHAFC,FEDEBITO,UBIC ";
                sSql = sSql + "FROM RENGLONES WITH (NOLOCK)  ";
                sSql = sSql + "WHERE IDCAB = '" + xIDCabecera + "' ";
                sSql = sSql + "ORDER BY FEPREST ";

                using (SqlCommand cmd2 = new SqlCommand(sSql, con))
                {
                    DataTable dt2 = new DataTable();
                    try
                    {
                        SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                        da2.Fill(dt2);

                        int Contador = dt2.Rows.Count;

                        if (dt2.Rows.Count > 0)
                        {
                            oDocPend = "[";
                            for (int i = 0; i < dt2.Rows.Count; i++)
                            {
                                
                                    oDocPend = oDocPend + "{";
                                    if (dt2.Rows[i]["NRORC"].ToString() == null || dt2.Rows[i]["NRORC"].ToString() == ""
                                        || dt2.Rows[i]["FERC"].ToString() == null || dt2.Rows[i]["FERC"].ToString() == "")
                                    {
                                        oDocPend = oDocPend + '"'+ "Estado" + '"' + ":" + '"' + "Pendiente" +'"' + ",";
                                    }
                                    else
                                    {
                                        oDocPend = oDocPend + '"' + "Estado" + '"' + ":" + '"' + "Obtenido" + '"' + ",";
                                    }
                                    oDocPend = oDocPend + '"' + "ID" + '"' + ":" + '"' + dt2.Rows[i]["ID"].ToString() + '"' + ",";
                                    oDocPend = oDocPend + '"' + "Prestador" + '"' + ":" + '"' + dt2.Rows[i]["NOMPRESTADOR"].ToString() + '"' + ",";
                                    oDocPend = oDocPend + '"' + "NroFC" + '"' + ":" + '"' + dt2.Rows[i]["NROFC"].ToString() + '"' + ",";
                                    oDocPend = oDocPend + '"' + "NroRC" + '"' + ":" + '"' + dt2.Rows[i]["NroRC"].ToString() + '"' + ",";
                                    oDocPend = oDocPend + '"' + "FechaRC" + '"' + ":" + '"' + FormatoFecha(dt2.Rows[i]["FERC"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    oDocPend = oDocPend + '"' + "FechaDebito" + '"' + ":" + '"' + FormatoFecha(dt2.Rows[i]["FEDEBITO"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    oDocPend = oDocPend + '"' + "FechaFC" + '"' + ":" + '"' + FormatoFecha(dt2.Rows[i]["FECHAFC"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    oDocPend = oDocPend + '"' + "Practica" + '"' + ":" + '"' + dt2.Rows[i]["ORDENPRACTICA"].ToString() + '"' + ",";
                                    oDocPend = oDocPend + '"' + "Tramite" + '"' + ":" + '"' + dt2.Rows[i]["NROTRAMITE"].ToString() + '"' + ",";
                                    oDocPend = oDocPend + '"' + "Fecierre" + '"' + ":" + '"' + FormatoFecha(dt2.Rows[i]["FECIERRE"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    oDocPend = oDocPend + '"' + "Importe" + '"' + ":" + '"' + dt2.Rows[i]["Importe"].ToString() + '"' + ",";
                                    oDocPend = oDocPend + '"' + "Ubicacion" + '"' + ":" + '"' + dt2.Rows[i]["UBIC"].ToString() + '"' + ",";
                                    oDocPend = oDocPend + '"' + "FEPREST" + '"' + ":" + '"' + FormatoFecha(dt2.Rows[i]["FEPREST"].ToString(), "dd/mm/yyyy", false) + '"';

                                    if (i < Contador - 1)
                                    {
                                        oDocPend = oDocPend + "},";
                                    }
                                    else
                                    {
                                        oDocPend = oDocPend + "}";
                                    };
                                    bandera = 1;
                                
                            }
                            oDocPend = oDocPend + "]";

                        }
                    }
                    catch (SqlException ex){
                        con.Close();
                        return "Error al procesar su petición: " + ex;
                    }

                    if (bandera == 0)
                    {
                        con.Close();
                        return "[]";
                    }
                    con.Close();
                    return oDocPend;
                }
            }
        }
        public string ModificacionConsumo(string xIDCab, string xIDCons, string xConcepto, string xTipo, string xNroAfiliado, string xNroAfiliado2, string xFePrest, string xOrdenPractica, string xNroTramite, string xFeCierre, string xImportePrest, string xNroFaC, string xFeFactura, string xImporte, string xNroRecibo, string xFeRecibo, string xFeDebito, string xCodPrestador, string xNombrePrest, string xNroRemito, string xFeRemito, string xNroEgreso, string xFeEgreso, string xNroCheque, string xBanco, string xCuenta, string xCodEsp, string xModulo, string xBase, string xUserLog, string xFilialLog, string xHayModifRecibo, string[] xNroReng, string[] xAnticipo, string[] xEstado, string[] xNroReng2, string[] xCant2, string[] xIdMed, string[] xPrecio, string[] xEstado2)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string sSql2 = "";
            string sSql3 = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xfilAfil = "";
            string xnroAfil = "";
            string xBeneAfil = "";
            string xfilAfil2 = "";
            string xnroAfil2 = "";
            string xBeneAfil2 = "";

            int bandera = 0;
            string Log_Afil = "";
            string Log_Codconc = "";
            string Log_Feprest = "";
            string Log_Fechafc = "";
            string Log_Nrofc = "";
            string Log_Fecharc = "";
            string Log_Nrorc = "";
            string Log_Ferm = "";
            string Log_Nrorm = "";
            string Log_Fecbteegreso = "";
            string Log_Nrocbteegreso = "";
            string Log_Codprestador = "";
            string Log_Nomprestador = "";

            string xUBICADM = "";
            string xESTADOADM = "";
            string xdblImportePrest = "";

            //Boolean HayModifMedic = false;
            Boolean xHayModifMedic = false;

            string xIDCAB_S2 = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xNroFaC) == true
                  || EsStrNuloBlanco(xFeFactura) == true
                  || EsStrNuloBlanco(xNombrePrest) == true
                  || EsStrNuloBlanco(xNroRemito) == true
                  || EsStrNuloBlanco(xFeRemito) == true
                  || EsStrNuloBlanco(xFePrest) == true
                  || EsStrNuloBlanco(xNroAfiliado) == true
                  || EsStrNuloBlanco(xNroAfiliado2) == true
                  || EsStrNuloBlanco(xNroEgreso) == true
                  || EsStrNuloBlanco(xFeEgreso) == true
                  || EsStrNuloBlanco(xNroCheque) == true
                  || EsStrNuloBlanco(xBanco) == true
                  || EsStrNuloBlanco(xCuenta) == true
                   )
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Existen campos obligatorios incompletos, imposible continuar." + '"' + "}]";
                return xMensaje;

            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT RENGLONES.* FROM RENGLONES WITH(NOLOCK)";
                sSql = sSql + " LEFT OUTER JOIN GRAL_AFILIADOS ON RENGLONES.FILAFIL = GRAL_AFILIADOS.FILAFIL AND RENGLONES.NROAFIL = GRAL_AFILIADOS.NROAFIL AND RENGLONES.BENEFAFIL = GRAL_AFILIADOS.BENEFAFIL";
                sSql = sSql + " WHERE ID = " + xIDCons;

                //Ejecutar el Select y asignar los valores siguientes a las variables: 
                //Tener encuenta que el Afiliado esta compuesto por FILAFIL + NROAFIL + BENEFAFIL 
                //y son 2 digitos + 7 digitos + 2 digitos si alguno tiene menos completar con ceros a la izquierda.

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;

                        if (dt.Rows.Count > 0)
                        {

                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                xfilAfil = dt.Rows[i]["filafil"].ToString().PadLeft(2, '0');
                                xnroAfil = dt.Rows[i]["nroafil"].ToString().PadLeft(7, '0');
                                xBeneAfil = dt.Rows[i]["benefafil"].ToString().PadLeft(2, '0');

                                Log_Afil = xfilAfil + xnroAfil + xBeneAfil;
                                Log_Codconc = dt.Rows[i]["CODCONC"].ToString();
                                Log_Feprest = dt.Rows[i]["FEPREST"].ToString();
                                Log_Fechafc = dt.Rows[i]["FECHAFC"].ToString();
                                Log_Nrofc = dt.Rows[i]["NROFC"].ToString();
                                Log_Fecharc = dt.Rows[i]["FERC"].ToString();
                                Log_Nrorc = dt.Rows[i]["NRORC"].ToString();
                                Log_Ferm = dt.Rows[i]["FERM"].ToString();
                                Log_Nrorm = dt.Rows[i]["NRORM"].ToString();
                                Log_Fecbteegreso = dt.Rows[i]["FECBTEEGRESO"].ToString();
                                Log_Nrocbteegreso = dt.Rows[i]["NROCBTEEGRESO"].ToString();
                                Log_Codprestador = dt.Rows[i]["CODPRESTADOR"].ToString();
                                Log_Nomprestador = dt.Rows[i]["NOMPRESTADOR"].ToString();

                                bandera = 1;
                            }

                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close();
                        return "Error al procesar su petición: " + ex;
                    }
                    //----------------------------------------
                    // variables auxiliares para los codigos de afiliados...
                    xfilAfil = xNroAfiliado.Substring(0, 2);
                    xnroAfil = xNroAfiliado.Substring(2, 7);
                    xBeneAfil = xNroAfiliado.Substring(9, 2);

                    xfilAfil2 = xNroAfiliado2.Substring(0, 2);
                    xnroAfil2 = xNroAfiliado2.Substring(2, 7);
                    xBeneAfil2 = xNroAfiliado2.Substring(9, 2);

                    //Si la validación anterior salió bien continuo:

                    sSql = "SELECT ID,UBICADM, ESTADOADM FROM CABECERAS   WITH(NOLOCK)";
                    sSql = sSql + " WHERE  FILAFIL = " + xfilAfil + "  AND NROAFIL =" + xnroAfil + "  AND BENEFAFIL =" + xBeneAfil;
                    sSql = sSql + " AND   CODCONC  = " + xConcepto;
                    sSql = sSql + " AND   ESTADOCBLE ='EN GESTION'";
                    sSql = sSql + " AND FEDESDE <= '" + FormatoFecha(xFePrest, "yyyy/mm/dd", false) + "'";
                    sSql = sSql + " AND FEHASTA >= '" + FormatoFecha(xFePrest, "yyyy/mm/dd", false) + "'";

                    using (SqlCommand cmd2 = new SqlCommand(sSql, con))
                    {
                        DataTable dt2 = new DataTable();
                        try
                        {
                            SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                            da2.Fill(dt2);

                            int Contador = dt2.Rows.Count;

                            if (dt2.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt2.Rows.Count; i++)
                                {
                                    xIDCab = dt2.Rows[i]["ID"].ToString();
                                    xUBICADM = dt2.Rows[i]["UBICADM"].ToString();
                                    xESTADOADM = dt2.Rows[i]["ESTADOADM"].ToString();
                                }
                            }
                            else
                            {
                                xIDCab = "0";
                            }
                        }
                        catch (SqlException ex)
                        {
                            con.Close();
                            return "Error al procesar su petición: " + ex;
                        }


                        con.Close();

                    }
                }


                // COMIENZO DEL PROCESO ....

                if (xTipo == "PRESTACION")
                {
                    Boolean asociaMedicOk = VerificarMediaAsocAotroConc(xIDCons, xConcepto, xModulo, xBase, xUserLog, xFilialLog);
                    if (asociaMedicOk == true)
                    {
                        xMensaje = "Hay medicamentos que no corresponden al concepto seleccionado, eliminarlos para poder aceptar.";
                        return xMensaje;
                    }

                    // S2 =  SELECT * FROM RENGLONES  WITH(NOLOCK) WHERE ID = [IDCons]
                    sSql = " SELECT * FROM RENGLONES  WITH(NOLOCK) WHERE ID =" + xIDCons;
                    using (SqlCommand cmd3 = new SqlCommand(sSql, con))
                    {
                        DataTable dt3 = new DataTable();
                        try
                        {
                            SqlDataAdapter da3 = new SqlDataAdapter(cmd3);
                            da3.Fill(dt3);

                            int Contador = dt3.Rows.Count;
                            if (dt3.Rows.Count > 0)
                            {
                                //    dblImportePrest = S2.IMPORTEPREST
                                for (int i = 0; i < dt3.Rows.Count; i++)
                                {
                                    xdblImportePrest = dt3.Rows[i]["IMPORTEPREST"].ToString();
                                    //    Si S2 no es EOF (Me fijo si el select trajo algún registro)
                                    //        Si  vIDCAB <>  S2.IDCAB (Compara el IDCAB que guarde con el que trajo el select S2)

                                    xIDCAB_S2 = dt3.Rows[i]["IDCAB"].ToString();
                                    if (xIDCab != xIDCAB_S2)
                                    {
                                        sSql2 = sSql2 + "," + "IDCAB=" + xIDCab;
                                        if (dt3.Rows[i]["ASTOCONTAB"].ToString() != "" || string.IsNullOrEmpty(dt3.Rows[i]["ASTOCONTAB"].ToString()))
                                        {
                                            sSql2 = sSql2 + ",ASTOCONTAB=NULL";
                                            sSql2 = sSql2 + ",FECONTAB=NULL";

                                            sSql = "INSERT INTO CONTABILIDAD SELECT IDCAB,IDRENG,CODOS,";
                                            sSql = sSql + "TIPORDO, 'PEND' AS NROASTO, NULL AS FEASTO,FILDEBITO,";
                                            sSql = sSql + "IMPORTE*-1 AS IMPORTE , 'P' AS TIPOASTO, IMPORTECRED*-1 AS IMPORTECRED,";
                                            sSql = sSql + "IMPORTEPREV*-1 AS IMPORTEPREV,IMPORTERDO*-1 AS IMPORTERDO,";
                                            sSql = sSql + "'SI' AS IMPORTADO,NULL AS ASTOSAP," + xFilialLog + " AS FILORIGEN,CAP,PLAN_AFIL";
                                            sSql = sSql + " FROM CONTABILIDAD  WITH(NOLOCK) ";
                                            sSql = sSql + " WHERE IDRENG=" + xIDCons;
                                            //Comienzo la transacción...

                                            con.Open();
                                            SqlCommand command = con.CreateCommand();
                                            SqlTransaction transaction;
                                            transaction = con.BeginTransaction("InsertContabilidad");

                                            try
                                            {
                                                command.Connection = con;
                                                command.Transaction = transaction;

                                                command.CommandText = sSql;
                                                command.ExecuteNonQuery();
                                                transaction.Commit();

                                            }
                                            catch (Exception ex)
                                            {
                                                // Algo salio mal, hago roll back de la transaccion.
                                                transaction.Rollback("InsertContabilidad");
                                                xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + "}]";
                                            }

                                        }
                                        //FIN Si  (S2.ASTOCONTAB <> ""  O distinto de NULL)
                                    }
                                    else//vIDCAB = S2.IDCAB
                                    {
                                        if (xIDCab == dt3.Rows[i]["IDCAB"].ToString())
                                        {
                                            if (xImporte != xImportePrest)
                                            {
                                                sSql2 = "";
                                                sSql2 = sSql2 + ",IDCAB = " + xIDCab;
                                                if (dt3.Rows[i]["ASTOCONTAB"].ToString() != "")
                                                {
                                                    sSql2 = sSql2 + ",ASTOCONTAB=NULL";
                                                    sSql2 = sSql2 + ",FECONTAB=NULL";

                                                    sSql = "INSERT INTO CONTABILIDAD";
                                                    sSql = sSql + " SELECT IDCAB,IDRENG,CODOS,";
                                                    sSql = sSql + " TIPORDO, 'PEND' AS NROASTO, NULL AS FEASTO,FILDEBITO,";
                                                    sSql = sSql + " IMPORTE*-1 AS IMPORTE , 'P' AS TIPOASTO, IMPORTECRED*-1 AS IMPORTECRED,";
                                                    sSql = sSql + " IMPORTEPREV*-1 AS IMPORTEPREV,IMPORTERDO*-1 AS IMPORTERDO, ";
                                                    sSql = sSql + " 'SI' AS IMPORTADO,NULL AS ASTOSAP," + xFilialLog + " as FILORIGEN,CAP,PLAN_AFIL";
                                                    sSql = sSql + " FROM CONTABILIDAD  WITH(NOLOCK) ";
                                                    sSql = sSql + " WHERE IDRENG= " + xIDCAB_S2;

                                                    //Comienzo la transacción...

                                                    con.Open();
                                                    SqlCommand command = con.CreateCommand();
                                                    SqlTransaction transaction;

                                                    transaction = con.BeginTransaction("InsertContabilidad2");
                                                    try
                                                    {
                                                        command.Connection = con;
                                                        command.Transaction = transaction;

                                                        command.CommandText = sSql;
                                                        command.ExecuteNonQuery();
                                                        transaction.Commit();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        // Algo salio mal, hago roll back de la transaccion.
                                                        transaction.Rollback("InsertContabilidad2");
                                                        xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + ex.ToString() + "}]";
                                                    }
                                                }
                                            }
                                        }


                                    }


                                }
                            }
                            con.Close(); // (cierro el Select 2 S2)
                        }
                        catch (SqlException ex) // DEL SELECT 
                        {
                            con.Close();
                            return "Error al procesar su petición: " + ex;
                        }

                        //vFilialAfil2 (Es la filial del afiliado ingresado y calculado mas arriba)
                        //vNroAfil2 (Es el número del afiliado ingresado y calculado mas arriba)
                        //vBenfAfil2 (Es el número de beneficiario del afiliado ingresado y calculado mas arriba)

                        sSql = "UPDATE RENGLONES SET ";
                        sSql = sSql + "CODCONC     =" + xConcepto + ",";
                        sSql = sSql + "FEPREST     ='" + FormatoFecha(xFePrest, "mm/dd/yyyy", false) + "',";

                        if (xOrdenPractica == "" || string.IsNullOrEmpty(xOrdenPractica))
                        {
                            sSql = sSql + "ORDENPRACTICA=NULL,";
                        }
                        else
                        {
                            sSql = sSql + "ORDENPRACTICA='" + xOrdenPractica + "',";
                        }
                        if (xNroTramite == "" || string.IsNullOrEmpty(xNroTramite))
                        {
                            sSql = sSql + "NROTRAMITE=NULL,";
                        }
                        else
                        {
                            sSql = sSql + "NROTRAMITE='" + xNroTramite + "',";
                        }
                        if (xFeCierre == "" || string.IsNullOrEmpty(xFeCierre))
                        {
                            sSql = sSql + "FECIERRE    =NULL,";
                        }
                        else
                        {
                            sSql = sSql + "FECIERRE    ='" + FormatoFecha(xFeCierre, "yyyy/mm/dd", false) + "',";
                        }

                        sSql = sSql + "FILAFIL2    =" + xfilAfil2 + ",";
                        sSql = sSql + "NROAFIL2    =" + xnroAfil2 + ",";
                        sSql = sSql + "BENEFAFIL2  =" + xBeneAfil2 + ",";
                        sSql = sSql + "IMPORTEPREST=" + xImportePrest + ",";
                        sSql = sSql + "MODIMPORT   = CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END,";
                        sSql = sSql + "USMOD       ='" + xUserLog + "',";
                        sSql = sSql + "FEMOD       ='" + DateTime.Now.ToString("yyyy/MM/dd") + "',";
                        sSql = sSql + "HORAMOD     ='" + DateTime.Now.ToString("HH:mm:ss") + "'";
                        // Se agrega la variable sSql2
                        sSql = sSql + sSql2;

                        sSql = sSql + " WHERE   ID = " + xIDCons;
                        //Comienzo la transacción...

                        con.Open();
                        SqlCommand command2 = con.CreateCommand();
                        SqlTransaction transaction2;

                        transaction2 = con.BeginTransaction("UpdateRenglones");
                        try
                        {
                            command2.Connection = con;
                            command2.Transaction = transaction2;

                            command2.CommandText = sSql;
                            command2.ExecuteNonQuery();
                            transaction2.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction2.Rollback("UpdateRenglones");
                            xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + ex.ToString() + "}]";
                        }

                    }
                    //Acá Termina el IF para validar Si  Tipo = PRESTACION
                    //------------------------------------------------------------------------------------------------------------------
                }
                else // PRESTACION
                {
                    if (xTipo == "DOCUMENTO")
                    {
                        //Generar una variable sSQL a la cual se le va a ir asignando la consulta Update que se va a armar:
                        sSql = "UPDATE RENGLONES";
                        sSql = sSql + " SET ";
                        sSql = sSql + " FILAFIL     =" + xfilAfil + ",";
                        sSql = sSql + " NROAFIL     = " + xnroAfil + ",";
                        sSql = sSql + " BENEFAFIL   = " + xBeneAfil + ",";
                        sSql = sSql + " NROFC       ='" + xNroFaC + "',";
                        sSql = sSql + " FECHAFC     ='" + FormatoFecha(xFeFactura, "yyyy/mm/dd", false) + "',";
                        sSql = sSql + " IMPORTE     = " + xImporte + ",";
                        if (xCodPrestador == "" || string.IsNullOrEmpty(xCodPrestador))
                        {
                            sSql = sSql + "CODPRESTADOR=NULL,";
                        }
                        else
                        {
                            sSql = sSql + "CODPRESTADOR='" + xCodPrestador + "',";
                        }
                        sSql = sSql + " NOMPRESTADOR='" + xNombrePrest + "',";
                        sSql = sSql + " NRORM       ='" + xNroRemito + "',";
                        if (string.IsNullOrEmpty(xFeRemito))
                        {
                            sSql = sSql + " FERM        =NULL,";
                        }
                        else
                        {
                            sSql = sSql + " FERM        ='" + FormatoFecha(xFeRemito, "yyyy/mm/dd", false) + "',";
                        }

                        sSql = sSql + " NROCBTEEGRESO='" + xNroEgreso + "',         ";
                        if (string.IsNullOrEmpty(xFeEgreso))
                        {
                            sSql = sSql + " FECBTEEGRESO        =NULL,";
                        }
                        else
                        {
                            sSql = sSql + " FECBTEEGRESO='" + FormatoFecha(xFeEgreso, "yyyy/mm/dd", false) + "',";
                        }

                        sSql = sSql + " NROCHEQUE   ='" + xNroCheque + "',";
                        sSql = sSql + " BANCO       ='" + xBanco + "',";
                        sSql = sSql + " NROCUENTA   ='" + xCuenta + "',";
                        sSql = sSql + " MODIMPORT   = CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END,         ";
                        sSql = sSql + " USMOD       ='" + xUserLog + "',";
                        sSql = sSql + " FEMOD       ='" + DateTime.Now.ToString("yyyy/MM/dd") + "',";
                        sSql = sSql + " HORAMOD     ='" + DateTime.Now.ToString("HH:mm:ss") + "',";
                        sSql = sSql + " LIBROEGRESO = CASE WHEN LIBROEGRESO <> '' then LIBROEGRESO + 'N' else NULL END ";

                        //    Se tiene que validar lo siguiente para ver si dentro del SET se dos 	campos mas 
                        //    SI [Concepto]= "DISCAPACIDAD" And [CodModEsp] <> "" Then

                        if (xConcepto == "DISCAPACIDAD" && xCodEsp != "")
                        {
                            sSql3 = "SELECT DISTINCT CODESP, CODMOD FROM CAB_DOCUMENTACION_PRESUP WITH(NOLOCK)";
                            sSql3 = sSql3 + " WHERE IDCAB IN (SELECT IDCAB FROM RENGLONES ";
                            sSql3 = sSql3 + " WHERE ID=" + xIDCons + ") AND CUIT='" + xCodPrestador + "' AND ";
                            sSql3 = sSql3 + " CODMOD IS NOT NULL AND CODESP IS NOT NULL AND CODMOD <>'' AND CODESP<>'' ";
                            sSql3 = sSql3 + " AND CODMOD  + ' - ' + CODESP   = '" + xCodEsp + "'";
                            sSql3 = sSql3 + " WHERE ID = " + xIDCons + "";

                            using (SqlCommand cmd2 = new SqlCommand(sSql3, con))
                            {
                                DataTable dt2 = new DataTable();
                                try
                                {
                                    SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                                    da2.Fill(dt2);

                                    int Contador = dt2.Rows.Count;

                                    if (dt2.Rows.Count > 0)
                                    {
                                        //Hacer un IF: SI el select trae registro concateno las dos sig lineas para el Update
                                        sSql = sSql + " ,CODMOD ='" + dt2.Rows[0]["CODMOD"].ToString() + "'";
                                        sSql = sSql + " ,CODESP ='" + dt2.Rows[0]["CODESP"].ToString() + "'";
                                    }

                                }
                                catch (SqlException ex)
                                {
                                    con.Close();
                                    return "Error al procesar su petición: " + ex;
                                }
                            }
                        }// END IF DE   if (xConcepto == "DISCAPACIDAD" && xCodEsp != "")

                        // Si se ejecuto el select de arriba o no, ejecuto el UPDATE igual
                        // completando con el WHERE...

                        sSql = sSql + " WHERE ID = " + xIDCons;

                        //Ejecutar el sSQL 
                        //Comienzo la transacción...

                        con.Open();
                        SqlCommand command = con.CreateCommand();
                        SqlTransaction transaction;

                        transaction = con.BeginTransaction("UpdateRenglones3");
                        try
                        {
                            command.Connection = con;
                            command.Transaction = transaction;

                            command.CommandText = sSql;
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback("UpdateRenglones3");
                            xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + ex + "}]";
                            con.Close();
                            return xMensaje;
                        }

                    }

                    else //DOCUMENTO
                    {
                        if (xTipo == "PAGO")
                        {
                            sSql = "UPDATE RENGLONES SET ";
                            if (xNroRecibo == "" || string.IsNullOrEmpty(xNroRecibo))
                            {
                                sSql = sSql + " NRORC       = NULL,";
                            }
                            else
                            {
                                sSql = sSql + " NRORC       = '" + xNroRecibo + "',";
                            }
                            if (xFeRecibo == "" || string.IsNullOrEmpty(xFeRecibo))
                            {
                                sSql = sSql + " FERC       = NULL,";
                            }
                            else
                            {
                                sSql = sSql + " FERC        = '" + FormatoFecha(xFeRecibo, "yyyy/mm/dd", false) + "',";
                            }
                            if (xFeDebito == "" || string.IsNullOrEmpty(xFeDebito))
                            {
                                sSql = sSql + " FEDEBITO       = NULL,";
                            }
                            else
                            {
                                sSql = sSql + " FEDEBITO    = '" + FormatoFecha(xFeDebito, "yyyy/mm/dd", false) + "',";
                            }
                            sSql = sSql + " LIBROEGRESO = CASE LIBROEGRESO WHEN '' then LIBROEGRESO + 'N' else NULL END,";
                            sSql = sSql + " MODIMPORT   = CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END,";
                            sSql = sSql + " USMOD       ='" + xUserLog + "',";
                            sSql = sSql + " FEMOD       ='" + DateTime.Now.ToString("yyyy/MM/dd") + "',";
                            sSql = sSql + " HORAMOD     ='" + DateTime.Now.ToString("HH:mm:ss") + "'";
                            sSql = sSql + " WHERE ID    =" + xIDCons;

                            //Comienzo la transacción...

                            con.Open();
                            SqlCommand command = con.CreateCommand();
                            SqlTransaction transaction;

                            transaction = con.BeginTransaction("UpdateRenglones4");
                            try
                            {
                                command.Connection = con;
                                command.Transaction = transaction;

                                command.CommandText = sSql;
                                command.ExecuteNonQuery();
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback("UpdateRenglones4");
                                xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + ex + "}]";
                                return xMensaje;
                            }

                        }//Fin xTIPO=PAGO
                    }//Fin xTIPO=DOCUMENTO
                }//Fin xTIPO=PRESTACION

                con.Close();
                //Array anticipo:  
                //&NRORENG:[NroRenglon]&Anticipo:[Anticipo]&Estado:[Estado]
                //Recorrer el Array de Anticipos y dentro del FOR poner el SI de Estados = Baja: 

                //SI [Estado] = BAJA
                try // 2) UPDATE de arrays de RENG_RECIBOSANTICIPOS
                {
                    con.Open();
                    for (int j = 0; j < xAnticipo.Length; j++)
                    {
                        if (xEstado[j].ToString() == "BAJA")
                        {
                            sSql = "UPDATE RENG_RECIBOSANTICIPOS ";
                            sSql = sSql + " SET ESTADO='BAJA', ";
                            sSql = sSql + " MODIMPORT = CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END ";
                            sSql = sSql + " WHERE IDRENG  = " + xIDCons + " AND NRORENG = " + xNroReng[j].ToString();

                            //Comienzo la transacción...
                            SqlTransaction transaction;
                            transaction = con.BeginTransaction("UpdateRecibosAnticipos");
                            try
                            {

                                SqlCommand command = con.CreateCommand();

                                command.CommandText = sSql;
                                command.Transaction = transaction;
                                command.ExecuteNonQuery();
                                // Commiteo todos los Inserts

                                transaction.Commit();
                            }
                            //-----
                            catch (Exception ex)
                            {
                                transaction.Rollback("UpdateRecibosAnticipos");
                                xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + ex + "}]";
                                return xMensaje;
                            }

                        }
                        //--------------------------------------------------------------------------------
                        if (xHayModifRecibo == "TRUE")
                        {
                            //S1
                            sSql = "SELECT ID FROM RENGLONES  WITH(NOLOCK)  ";
                            sSql = sSql + " WHERE   NROCBTEEGRESO='" + xNroEgreso + "' AND ID <> " + xIDCons;

                            SqlTransaction transaction;
                            //transaction = con.BeginTransaction("UpdateRengRecibosAnticipos");
                            // Recorrer con un For el select antarior y hacer el sig Update para cada ID encontrado:
                            using (SqlCommand cmd2 = new SqlCommand(sSql, con))
                            {
                                DataTable dt2 = new DataTable();
                                try
                                {
                                    SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                                    da2.Fill(dt2);

                                    int Contador = dt2.Rows.Count;

                                    if (dt2.Rows.Count > 0)
                                    {
                                        //SqlTransaction transaction;
                                        transaction = con.BeginTransaction("UPDATERENG_RECIBOSANTICIPOS");
                                        con.Open();
                                        for (int k = 0; k < dt2.Rows.Count; k++)
                                        {
                                            try
                                            {
                                                sSql2 = "UPDATE RENG_RECIBOSANTICIPOS ";
                                                sSql2 = sSql2 + " SET ESTADO='BAJA', MODIMPORT = CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END ";
                                                sSql2 = sSql2 + " WHERE IDRENG  = " + dt2.Rows[k]["ID"].ToString() + "'";
                                                //Comienzo la transacción...

                                                //con.Open();
                                                SqlCommand command = con.CreateCommand();

                                                command.CommandText = sSql2;
                                                command.Transaction = transaction;
                                                command.ExecuteNonQuery();
                                                transaction.Commit();
                                            }
                                            //--
                                            catch (Exception ex)
                                            {
                                                transaction.Rollback("UPDATERENG_RECIBOSANTICIPOS");
                                                xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + ex + "}]";
                                                return xMensaje;
                                            }

                                        }
                                        con.Close();
                                        //transaction.Commit();
                                    }

                                }
                                catch (SqlException ex)
                                {
                                    con.Close();
                                    return "Error al procesar su petición: " + ex;
                                }

                            }


                        }//Fin FOR
                        //con.Close();

                        //Fin SI
                        //Fin Si

                        //Fin del For
                    } // Fin del TRY
                    con.Close();
                    // ARRAYS -------------------------------------------------------------------------------

                    //Los FOR para recorrer el array de anticipos en los dos caso son iguales pero hacerlos por separado como se indica.
                    //Array anticipo:  
                    //&NRORENG:[NroRenglon]&Anticipo:[Anticipo]&Estado:[Estado]
                    //Recorrer el Arrar de Anticipos y dentro del FOR poner el SI de Estados = Alta: 

                    //SI [Estado] = ALTA
                    try // 2) UPDATE de arrays de RENG_RECIBOSANTICIPOS
                    {
                        for (int j = 0; j < xAnticipo.Length; j++)
                        {
                            if (xEstado[j].ToString() == "ALTA")
                            {

                                sSql = "Select isnull(Max(NRORENG),0)+1 AS Maximo FROM RENG_RECIBOSANTICIPOS";
                                sSql = sSql + " WHERE IDRENG = " + xIDCons;

                                string vNroReng = "";
                                using (SqlCommand cmd = new SqlCommand(sSql, con))
                                {
                                    try
                                    {
                                        con.Open();
                                        DataTable dt = new DataTable();
                                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                                        da.Fill(dt);
                                        if (dt.Rows.Count > 0)
                                        {
                                            vNroReng = (dt.Rows[0]["MAXIMO"].ToString());
                                        }
                                        else
                                        { vNroReng = "1"; }
                                    }
                                    catch (Exception ex)
                                    {
                                        vNroReng = "1";
                                        con.Close();
                                    }
                                    con.Close();
                                    try
                                    {
                                        SqlTransaction transaction;
                                        con.Open();
                                        transaction = con.BeginTransaction("INSERTINTORENG_RECIBOSANTICIPOS");

                                        try
                                        {
                                            sSql2 = "INSERT INTO RENG_RECIBOSANTICIPOS ";
                                            sSql2 = sSql2 + " VALUES (" + xIDCons + ","+ vNroReng+",'" + xAnticipo[j].ToString() + "',NULL,'NO','NO')";

                                            //Comienzo la transacción...
                                           
                                            SqlCommand command = con.CreateCommand();
                                            command.CommandText = sSql2;
                                            command.Transaction = transaction;
                                            command.ExecuteNonQuery();
                                            // Commiteo todos los Inserts

                                            transaction.Commit();
                                        }

                                        catch (SqlException ex)
                                        {

                                            transaction.Rollback("INSERTINTORENG_RECIBOSANTICIPOS");
                                            con.Close();
                                            return "Error al procesar su petición: " + ex;
                                        }
                                        con.Close();
                                    }
                                    catch (SqlException ex)
                                    {

                                        con.Close();
                                        return "Error al procesar su petición: " + ex;
                                    }

                                }
                            }


                            if (xHayModifRecibo == "true")
                            {
                                //S1
                                sSql = " SELECT ID FROM RENGLONES  WITH(NOLOCK)  ";
                                sSql = sSql + " WHERE   NROCBTEEGRESO='" + xNroEgreso + "' AND ID <> " + xIDCons;
                                con.Open();
                                using (SqlCommand cmd22 = new SqlCommand(sSql, con))
                                {
                                    DataTable dt22 = new DataTable();
                                    
                                    try
                                    {
                                        SqlDataAdapter da22 = new SqlDataAdapter(cmd22);
                                        da22.Fill(dt22);

                                        int Contador = dt22.Rows.Count;

                                        if (dt22.Rows.Count > 0)
                                        {
                                            //con.Open();
                                            SqlTransaction transaction;
                                            transaction = con.BeginTransaction("INSERTINTORENG_RECIBOSANTICIPOS");
                                            for (int jj = 0; jj < xNroReng.Length; jj++)
                                            {
                                                try
                                                {
                                                    sSql2 = "INSERT INTO RENG_RECIBOSANTICIPOS ";
                                                    sSql2 = sSql2 + " VALUES (" + dt22.Rows[jj]["ID"].ToString() + ","+ xNroReng[jj].ToString() +",'" + xAnticipo[jj].ToString() + "',NULL,'NO','NO')";
                                                    //Recorrer con un For el select antarior y hacer el sig Incerte para cada ID encontrado:
                                                    //Comienzo la transacción...


                                                    SqlCommand command = con.CreateCommand();

                                                    command.CommandText = sSql2;
                                                    command.Transaction = transaction;
                                                    command.ExecuteNonQuery();
                                                    // Commiteo todos los Inserts

                                                    transaction.Commit();
                                                }

                                                catch (SqlException ex)
                                                {

                                                    transaction.Rollback("INSERTINTORENG_RECIBOSANTICIPOS");
                                                    con.Close();
                                                    return "Error al procesar su petición: " + ex;
                                                }
                                            }
                                            con.Close();

                                        }
                                    }

                                    catch (Exception ex)
                                    {
                                        con.Close();
                                        return "Error al procesar su petición: " + ex;
                                    }

                                    //Pregunto SI HayModifRecibo = TRUE
                                    if (xHayModifRecibo == "true")
                                    {
                                       
                                        //con.Open();
                                        //transaction = con.BeginTransaction("INSERTINTORENG_RECIBOSANTICIPOS");
                                        try
                                        {
                                            SqlTransaction transaction;
                                            con.Open();
                                            transaction = con.BeginTransaction("UpdateRenglones");
                                            try
                                            {
                                                sSql = " UPDATE RENGLONES";
                                                sSql = sSql + " SET NRORC = '" + xNroRecibo + "',";
                                                sSql = sSql + " FERC = '" + FormatoFecha(xFeRecibo, "yyyy-mm-dd", false) + "',";
                                                sSql = sSql + " FEDEBITO    = '" + FormatoFecha(xFeDebito, "yyyy-mm-dd", false) + "',";
                                                sSql = sSql + " MODIMPORT = CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END,         ";
                                                sSql = sSql + " LIBROEGRESO = CASE LIBROEGRESO WHEN NULL then  NULL else LIBROEGRESO + 'N' END";
                                                sSql = sSql + " WHERE   NROCBTEEGRESO='" + xNroEgreso + "' AND ID <> " + xIDCons;

                                                //Comienzo la transacción...
                                                
                                                SqlCommand command = con.CreateCommand();

                                                
                                                command.CommandText = sSql;
                                                command.Transaction = transaction;
                                                command.ExecuteNonQuery();
                                                // Commiteo todos los Updates

                                                transaction.Commit();
                                            }
                                            catch (Exception ex)
                                            {

                                                // Algo salio mal, hago roll back de la transaccion.
                                                transaction.Rollback("UpdateRenglones");
                                                con.Close();
                                                return "Error al procesar su petición: " + ex;

                                            }
                                            con.Close();
                                        }
                                        catch (Exception ex)
                                        {

                                            // Algo salio mal, hago roll back de la transaccion.
                                            con.Close();
                                            return "Error al procesar su petición: " + ex;

                                        }

                                    }
                                }

                                //Fin del Si


                                //recorrer el array de medicamentos:
                                //Array med: &IDReng:[IDReng]&NroReng:[NroReng]&Cant:[Cantidad]&IDMed:[IDMed]&Precio:[Precio] &Estado:[Estado]

                                //Recorrer el array de medicamento con un For (IDMed):
                                //                   Boolean xHaymodifMedic=false;

                                //Dentro del for hacer los sig IF para validar si el Estado es baja o alta del registro:
                                con.Open();
                                for (int jjj = 0; jjj < xIdMed.Length; jjj++)
                                {
                                    SqlTransaction transaction;
                                    transaction = con.BeginTransaction("ModifMotivosTransaction");
                                    if (xEstado[jjj].ToString() == "BAJA")
                                    {
                                        xHayModifMedic = true;

                                        sSql = " UPDATE  RENG_DESCRIPCION  ";
                                        sSql = sSql + "  SET  ESTADO='BAJA', MODIMPORT = CASE IMPORTADO";
                                        sSql = sSql + "   WHEN 'NO' then 'NO' else 'SI' END ";
                                        sSql = sSql + "  WHERE IDRENG  =" + xIDCons + " AND NRORENG = " + xNroReng[jjj].ToString();
                                    }
                                    //Else
                                    else
                                    {
                                        xHayModifMedic = true;
                                        // ALPHA
                                        sSql = "INSERT INTO RENG_DESCRIPCION ";
                                        sSql = sSql + " VALUES (" + xIDCons + ",";
                                        sSql = sSql + xNroReng[jjj].ToString() + ",";
                                        sSql = sSql + xCant2[jjj].ToString() + ",";
                                        sSql = sSql + xIdMed[jjj].ToString() + ",'','',";
                                        sSql = sSql + xPrecio[jjj].ToString() + ",";
                                        sSql = sSql + "'" + xUserLog.ToString() + "',";
                                        sSql = sSql + "'" + DateTime.Now.ToString("yyyy/MM/dd") + "',";
                                        sSql = sSql + "'" + DateTime.Now.ToString("HH:mm:ss") + "',";
                                        sSql = sSql + "NULL,'NO','NO',NULL)";
                                        //Fin si
                                        //Fin Si
                                        //Paso al siguiente 
                                        //Acá termina el metodo que recorre el array de medicamentos
                                    }
                                    //Comienzo la transacción...
                                    try
                                    {

                                        SqlCommand command = con.CreateCommand();

                                        command.CommandText = sSql;
                                        command.Transaction = transaction;
                                        command.ExecuteNonQuery();
                                        // Commiteo todos los Inserts

                                        transaction.Commit();
                                    }

                                    catch (Exception ex)
                                    {

                                        // Algo salio mal, hago roll back de la transaccion.
                                        transaction.Rollback("ModifMotivosTransaction");
                                        con.Close();
                                        return "Su peticion no puede realizarse." + ex;

                                    }
                                }
                                con.Close();
                                //Acá termina el metodo que recorre el array de medicamentos

                                //Pregunta si HayModifMedic = TRUE
                                if (xHayModifMedic == true)
                                {
                                    sSql = "INSERT INTO CONTABILIDAD ";
                                    sSql = sSql + " SELECT IDCAB,IDRENG,CODOS, TIPORDO, 'PEND' AS NROASTO, NULL AS FEASTO,";
                                    sSql = sSql + " FILDEBITO, IMPORTE*-1 AS IMPORTE , 'P' AS TIPOASTO, IMPORTECRED*-1 AS IMPORTECRED,";
                                    sSql = sSql + " IMPORTEPREV*-1 AS IMPORTEPREV,IMPORTERDO*-1 AS IMPORTERDO, 'SI' AS IMPORTADO,";
                                    sSql = sSql + " NULL AS ASTOSAP, " + xFilialLog + " AS FILORIGEN ,CAP,PLAN_AFIL FROM CONTABILIDAD  WITH(NOLOCK) ";
                                    sSql = sSql + " WHERE IDRENG= " + xIDCons;
                                    SqlTransaction transaction;
                                    con.Open();
                                    transaction = con.BeginTransaction("INSERTINTOCONTABILIDAD2");
                                    try
                                    {
                                        //con.Open();
                                        SqlCommand command = con.CreateCommand();

                                        command.CommandText = sSql;
                                        command.Transaction = transaction;
                                        command.ExecuteNonQuery();
                                        // Commiteo todos los Inserts

                                        transaction.Commit();
                                    }

                                    catch (Exception ex)
                                    {

                                        // Algo salio mal, hago roll back de la transaccion.
                                        transaction.Rollback("INSERTINTOCONTABILIDAD2");
                                        con.Close();
                                        return "Su peticion no puede realizarse." + ex;
                                    }
                                    con.Close();
                                    sSql2 = "UPDATE RENGLONES SET  ASTOCONTAB=NULL, FECONTAB=NULL  WHERE ID = " + xIDCons;
                                    //SqlTransaction transaction;
                                    con.Open();
                                    transaction = con.BeginTransaction("ModifMotivosTransaction");
                                    try
                                    {

                                        //con.Open();
                                        SqlCommand command = con.CreateCommand();

                                        command.CommandText = sSql2;
                                        command.Transaction = transaction;
                                        command.ExecuteNonQuery();
                                        // Commiteo todos los Inserts

                                        transaction.Commit();
                                    }

                                    catch (Exception ex)
                                    {

                                        // Algo salio mal, hago roll back de la transaccion.
                                        transaction.Rollback("ModifMotivosTransaction");
                                        con.Close();
                                        return "Su peticion no puede realizarse." + ex;
                                    }
                                    con.Close();
                                }

                                //FIn del SI

                                //Una vez realizado todo lo anterior y por fuera de esto, hay que validar si se requiere dejar guardado en el log el cambio.
                                //Para poder determinar esto hay que comparar las variables Log creadas al principio con los parametros que nos fue entregado en el request.

                                if (Log_Afil == xNroAfiliado2 &&
                                   Log_Codconc == xConcepto &&
                                   Log_Feprest == xFePrest &&
                                   Log_Fechafc == xFeFactura &&
                                   Log_Nrofc == xNroFaC &&
                                   Log_Fecharc == xFeRecibo &&
                                   Log_Nrorc == xNroRecibo &&
                                   Log_Ferm == xFeRemito &&
                                   Log_Nrorm == xNroRemito &&
                                   Log_Fecbteegreso == xFeEgreso &&
                                   Log_Nrocbteegreso == xNroEgreso &&
                                   Log_Codprestador == xCodPrestador &&
                                   Log_Nomprestador == xNombrePrest)
                                {
                                    // Las variables son iguales a los parametros, entonces no hago nada...
                                }
                                else
                                {
                                    Grabar_Log(xIDCab, xIDCons, xfilAfil2, xnroAfil2, xBeneAfil2, Log_Codconc, Log_Feprest, Log_Fechafc, Log_Nrofc, Log_Fecharc, Log_Nrorc,
                                        Log_Ferm, Log_Nrorm, Log_Fecbteegreso, Log_Nrocbteegreso, Log_Codprestador, Log_Nomprestador, xModulo, xBase, xUserLog, xFilialLog);
                                }
                            }

                        }
                        xMensaje = "La Modificacion del consumo se realizo correctamente.";
                    }
                   
                    catch (Exception ex)
                    {
                        xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + ex + "}]";
                    }
                    return xMensaje;
                }
                catch (Exception ex)
                {
                    xMensaje = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "Error al intentar realizar su peticion." + "}]";
                }
                if (xUBICADM.ToString() != xFilialLog && xIDCab.ToString() != "0")
                {
                    xMensaje = "Atención: La ubicación Administrativa es diferente a la del consumo. Ubic Adm:" + xUBICADM.ToString() + " Estado Adm.:" + xESTADOADM.ToString();
                }
                else
                {
                    xMensaje = "La Modificacion del consumo se realizo correctamente.";
                }
                return xMensaje;
            }

        }

        //----------------------------------------------------------------------------------------------------------------------------
        // Funciones de uso interno
        string Grabar_Log(string xIDCab, string xIDCons, string xLog_FilAfil, string xLog_NroAfil, string xLog_BenefAfil, string xLog_Codconc, string xLog_Feprest, string xLog_Fechafc, string xLog_Nrofc, string xLog_Fecharc, string xLog_Nrorc, string xLog_Ferm, string xLog_Nrorm, string xLog_Fecbteegreso, string xLog_Nrocbteegreso, string xLog_Codprestador, string xLog_Nomprestador, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();

            string Grabar_Log_Ok = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "";
                sSql = sSql + "INSERT INTO Log_Cbios_Renglones ( ";
                sSql = sSql + "IDRENG,IDCAB,Filafil,Nroafil,Benefafil,Codconc,Feprest,Fechafc,";
                sSql = sSql + "Nrofc,Fecharc, Nrorc,Ferm ,Nrorm,Fecbteegreso,Nrocbteegreso,Codprestador,";
	            sSql = sSql + "Nomprestador,USUARIO,FEMOD)";
                sSql = sSql + " VALUES ('" + xIDCons + "',";
                sSql = sSql + "" + xIDCab + ",";
                sSql = sSql + "'" + xLog_FilAfil + "',";
                sSql = sSql + "'" + xLog_NroAfil + "',";
                sSql = sSql + "'" + xLog_BenefAfil + "',";
                sSql = sSql + "'" + xLog_Codconc + "',";
                sSql = sSql + "'" + FormatoFecha(xLog_Feprest, "yyyy/mm/dd", false) + "',";
                sSql = sSql + "'" + FormatoFecha(xLog_Fechafc, "yyyy/mm/dd", false) + "',";
                sSql = sSql + "'" + xLog_Nrofc + "',";
                sSql = sSql + "'" + FormatoFecha(xLog_Fecharc, "yyyy/mm/dd", false) + "',";
                
                sSql = sSql + "'" + xLog_Nrorc + "',";
                sSql = sSql + "'" + FormatoFecha(xLog_Ferm, "yyyy/mm/dd", false) + "',";

                sSql = sSql + "'" + xLog_Nrorm + "',";
                sSql = sSql + "'" + FormatoFecha(xLog_Fecbteegreso, "yyyy/mm/dd", false) + "',";
                sSql = sSql + "'" + xLog_Nrocbteegreso + "',";
                sSql = sSql + "'" + xLog_Codprestador + "',";
                sSql = sSql + "'" + xLog_Nomprestador + "',";
                sSql = sSql + "'" + xUserLog + "',";
                sSql = sSql + "'" + DateTime.Now.ToString("yyyy/MM/dd") + " " + DateTime.Now.ToString("HH:mm:ss") + "')";

                con.Open();
                // OBA
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("Grabar_Log");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    //-------------------------------------

                    Grabar_Log_Ok = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                    transaction.Commit();

                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("Grabar_Log");
                    Grabar_Log_Ok = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";

                }
                con.Close();
                return Grabar_Log_Ok;
            }
        }
        Boolean VerificarMediaAsocAotroConc(string xIdCons, string xConcepto, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            // Variables de entrada: [idCons], [Concepto], 
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            bool xVerificarMediaAsocAotroConc = false;
            string xCodconc = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return false;
            }

            if (EsStrNuloBlanco(xConcepto) == true ||
                EsValNumerico(xIdCons) == false)
            {
                return false;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT CodConc FROM RENGLONES WITH(NOLOCK) WHERE ID =" + xIdCons;

                using (SqlCommand cmdsd = new SqlCommand(sSql, con))
                {
                    DataTable dtsd = new DataTable();
                    try
                    {
                        SqlDataAdapter dasd = new SqlDataAdapter(cmdsd);
                        dasd.Fill(dtsd);
                        if (dtsd.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtsd.Rows.Count; i++)
                            {
                                //Tomar el Valor del campo CodConc y asignarlo a una variable.                              
                                xCodconc = dtsd.Rows[i]["codconc"].ToString();
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        return false;
                    }
                }

                //Preguntar:
                //SI ConceptoDB <> [Concepto]
                if (xCodconc != xConcepto)
                {

                    sSql = "SELECT RENGLONES.CODCONC AS CODCONC,RENG_DESCRIPCION.CODMEDIC AS CODMEDIC ";
                    sSql = sSql + " FROM RENGLONES  WITH(NOLOCK) JOIN RENG_DESCRIPCION  WITH(NOLOCK) ";
                    sSql = sSql + " ON RENGLONES.ID = RENG_DESCRIPCION.IDRENG";
                    sSql = sSql + " WHERE IDRENG = " + xIdCons + " And RENG_DESCRIPCION.Estado Is Null";

                    using (SqlCommand cmdsd2 = new SqlCommand(sSql, con))
                    {
                        DataTable dtsd2 = new DataTable();
                        try
                        {
                            SqlDataAdapter dasd2 = new SqlDataAdapter(cmdsd2);
                            dasd2.Fill(dtsd2);
                            if (dtsd2.Rows.Count > 0)
                            {
                                for (int j = 0; j < dtsd2.Rows.Count; j++)
                                {
                                    //Recorro todos los registros obtenidos de S2 y dentro del FOR:
                                    sSql = "( SELECT GRAL_MEDICAMENTOS.CODMEDIC, GRAL_MEDICAMENTOS.CODCONC";
                                    sSql = sSql + " FROM GRAL_MEDICAMENTOS  WITH(NOLOCK)";
                                    sSql = sSql + " WHERE GRAL_MEDICAMENTOS.codmedic =" + dtsd2.Rows[j]["codmedic"].ToString() + " and GRAL_MEDICAMENTOS.codConc = '" + xConcepto + "' )";

                                    using (SqlCommand cmdsd3 = new SqlCommand(sSql, con))
                                    {
                                        DataTable dtsd3 = new DataTable();
                                        try
                                        {
                                            SqlDataAdapter dasd3 = new SqlDataAdapter(cmdsd2);
                                            dasd3.Fill(dtsd3);

                                            if (dtsd3.Rows.Count > 0)
                                            {
                                                for (int y = 0; y < dtsd3.Rows.Count; y++)
                                                {
                                                    xVerificarMediaAsocAotroConc = false;

                                                }

                                            }
                                            // SI S3 No trae registros (Count = 0) entonces
                                            else
                                            {
                                                xVerificarMediaAsocAotroConc = true;
                                            }
                                        }
                                        catch (SqlException ex)
                                        {
                                            return false;
                                        }

                                    }
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            return false;
                        }


                    }
                }


            }

            //Cierro las conexiones

            //Termino el proceso y retorno VerificarMEDIAsocAOtroCONC
            return xVerificarMediaAsocAotroConc;
        }
        //------------------------------------------------------------------------------------------------------------------
    }
}
