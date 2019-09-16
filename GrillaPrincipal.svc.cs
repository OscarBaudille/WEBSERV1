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
    public class WS1 : GrillaPrincipal
    {

        public SqlConnection con { get; set; }public string xGP;
        public int xCantOs = 0;
        public string DiaVto = "0";
        public int[] DiasNaranja = new int[9];
        public int[] DiasRojo = new int[9];

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }
        [WebMethod]
        public string StringStatus(string value)
        {
            return value;
        }

        public string MensajeStatus1(string xValue)
        {
            return string.Format("Mensaje desde el WS: {0}", xValue);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public string PopulateDropDownListAgrup(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string jAgrup = "";

            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            int xContaErr = 0;
            StatusWs objStatus = new StatusWs();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT AGRUPCONC as ID_Agrup,AGRUPCONC as Agrup_Name FROM GRAL_CONCEPTOS WITH(NOLOCK) GROUP BY AGRUPCONC ORDER BY AGRUPCONC", con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            jAgrup = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                jAgrup = jAgrup + "{";
                                jAgrup = jAgrup + '"' + "ID_Agrup" + '"' + ":" + '"' + dt.Rows[i]["ID_Agrup"].ToString() + '"' + ",";
                                jAgrup = jAgrup + '"' + "Agrup_Name" + '"' + ":" + '"' + dt.Rows[i]["Agrup_Name"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    jAgrup = jAgrup + "},";
                                }
                                else
                                {
                                    jAgrup = jAgrup + "}";
                                };
                            }
                            jAgrup = jAgrup + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            jAgrup = "[]";
                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jAgrup = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jAgrup = jAgrup + "{";
                            jAgrup = jAgrup + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jAgrup = jAgrup + "}";
                            }
                            else
                            {
                                jAgrup = jAgrup + "},";
                            };
                        }
                        jAgrup = jAgrup + "]";
                    }
                    con.Close();
                    return jAgrup;
                }
            }
        }


        public string PopulateDropDownListCptos(string xCptoAgrup, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string sSql;
            string ConceptoAgrup = xCptoAgrup;
            string xSigno = "?";
            int xContaErr = 0;
            StatusWs objStatus = new StatusWs();
            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (xCptoAgrup == null || xCptoAgrup.ToString().Equals(xSigno) || xCptoAgrup.ToString().Equals("") || xCptoAgrup.ToString().Equals(" "))
            {
                sSql = "SELECT CODCONC AS IdCpto,DESCCONC AS CptoName FROM GRAL_CONCEPTOS WITH(NOLOCK) ORDER BY DESCCONC";

            }
            else
            {
                sSql = "SELECT CODCONC AS IdCpto,DESCCONC AS CptoName FROM GRAL_CONCEPTOS WITH(NOLOCK) WHERE AGRUPCONC IN ('" + ConceptoAgrup + "') ORDER BY DESCCONC";
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
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
                            xCptoAgrup = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                xCptoAgrup = xCptoAgrup + "{";
                                xCptoAgrup = xCptoAgrup + '"' + "IdCpto" + '"' + ":" + '"' + dt.Rows[i]["IdCpto"].ToString() + '"' + ",";
                                xCptoAgrup = xCptoAgrup + '"' + "CptoName" + '"' + ":" + '"' + dt.Rows[i]["CptoName"].ToString() + '"';
                                if (i < Contador - 1)
                                {
                                    xCptoAgrup = xCptoAgrup + "},";
                                }
                                else
                                {
                                    xCptoAgrup = xCptoAgrup + "}";
                                };
                            }
                            xCptoAgrup = xCptoAgrup + "]";

                        }
                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "AGRUPAMIENTO: NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            xCptoAgrup = "[]";
                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        xCptoAgrup = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            xCptoAgrup = xCptoAgrup + "{";
                            xCptoAgrup = xCptoAgrup + '"' + "ERROR" + '"' + " AGRUPAMIENTO:" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                xCptoAgrup = xCptoAgrup + "}";
                            }
                            else
                            {
                                xCptoAgrup = xCptoAgrup + "},";
                            };
                        }
                        xCptoAgrup = xCptoAgrup + "]";
                    }
                    con.Close();
                    return xCptoAgrup;
                }
            }
        }

        public string PopulateDropDownListOsocial(string xOS, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            int xContaErr = 0;
            StatusWs objStatus = new StatusWs();
            DataTable dt = new DataTable();
            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT RIGHT('000' + CODOS,3) AS CODOS, DESCOS FROM GRAL_OBRASSOCIALES WITH(NOLOCK) ORDER BY CODOS", con))
                {
                    try
                    {
                        con.Open();

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;
                        xCantOs = dt.Rows.Count;

                        if (dt.Rows.Count > 0)
                        {
                            xOS = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                xOS = xOS + "{";
                                xOS = xOS + '"' + "IdOs" + '"' + ":" + '"' + dt.Rows[i]["CODOS"].ToString() + '"' + ",";
                                xOS = xOS + '"' + "OsName" + '"' + ":" + '"' + dt.Rows[i]["CODOS"].ToString() + " - " + dt.Rows[i]["DESCOS"].ToString() + '"';
                                if (i < Contador - 1)
                                {
                                    xOS = xOS + "},";
                                }
                                else
                                {
                                    xOS = xOS + "}";
                                };
                            }
                            xOS = xOS + "]";
                        }
                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "OBRA SOCIAL:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            xOS = "[]";
                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        xOS = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            xOS = xOS + "{";
                            xOS = xOS + '"' + "ERROR" + '"' + " OBRA SOCIAL:" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                xOS = xOS + "}";
                            }
                            else
                            {
                                xOS = xOS + "},";
                            };
                        }
                        xOS = xOS + "]";
                    }
                    con.Close();
                    return xOS;
                }
            }
        }

        public string PopulateDropDownListAfiliacion(string xAfil, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            int xContaErr = 0;
            StatusWs objStatus = new StatusWs();
            DataTable dt = new DataTable();

            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT RIGHT('000' + RNOS,6) AS RNOS, DESCOS FROM GRAL_OBRASSOCIALES WITH(NOLOCK) ORDER BY RNOS", con))
                {
                    try
                    {
                        con.Open();

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;
                        //Para el Redimensionamiento de los arrays de DIAS...en el Metodode Grilla Principal...
                        xCantOs = dt.Rows.Count;

                        if (dt.Rows.Count > 0)
                        {
                            xAfil = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                xAfil = xAfil + "{";
                                xAfil = xAfil + '"' + "IdRnos" + '"' + ":" + '"' + dt.Rows[i]["RNOS"].ToString() + '"' + ",";
                                xAfil = xAfil + '"' + "RnosName" + '"' + ":" + '"' + dt.Rows[i]["RNOS"].ToString() + " - " + dt.Rows[i]["DESCOS"].ToString() + '"';
                                xAfil = xAfil + "},";
                            }
                            xAfil = xAfil + "{";
                            xAfil = xAfil + '"' + "IdRnos" + '"' + ":" + '"' + "S/EMP." + '"' + ",";
                            xAfil = xAfil + '"' + "RnosName" + '"' + ":" + '"' + "S/EMP." + '"';

                            xAfil = xAfil + "},";
                            xAfil = xAfil + "{";
                            xAfil = xAfil + '"' + "IdRnos" + '"' + ":" + '"' + "SIND." + '"' + ",";
                            xAfil = xAfil + '"' + "RnosName" + '"' + ":" + '"' + "SIND." + '"';

                            xAfil = xAfil + "},";
                            xAfil = xAfil + "{";
                            xAfil = xAfil + '"' + "IdRnos" + '"' + ":" + '"' + "CAUT." + '"' + ",";
                            xAfil = xAfil + '"' + "RnosName" + '"' + ":" + '"' + "CAUT." + '"';

                            xAfil = xAfil + "},";
                            xAfil = xAfil + "{";
                            xAfil = xAfil + '"' + "IdRnos" + '"' + ":" + '"' + "S/INFO" + '"' + ",";
                            xAfil = xAfil + '"' + "RnosName" + '"' + ":" + '"' + "S/INFO" + '"';
                            xAfil = xAfil + "},";
                            xAfil = xAfil + "{";
                            xAfil = xAfil + '"' + "IdRnos" + '"' + ":" + '"' + "O/O.SOC" + '"' + ",";
                            xAfil = xAfil + '"' + "RnosName" + '"' + ":" + '"' + "O/O.SOC" + '"';
                            xAfil = xAfil + "}";
                            xAfil = xAfil + "]";
                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "AFILIACION:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            xAfil = "[]";
                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        xAfil = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            xAfil = xAfil + "{";
                            xAfil = xAfil + '"' + "ERROR" + '"' + " AFILIACION:" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                xAfil = xAfil + "}";
                            }
                            else
                            {
                                xAfil = xAfil + "},";
                            };
                        }
                        xAfil = xAfil + "]";
                    }
                    con.Close();
                    return xAfil;
                }
            }
        }

        public string PopulateDropDownListFcreacion(string xFcrea, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            int xContaErr = 0;
            StatusWs objStatus = new StatusWs();
            DataTable dt = new DataTable();
            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM GRAL_FILIALES WITH(NOLOCK) WHERE ESTFIL = 'A' ORDER BY CODFIL", con))
                {
                    try
                    {
                        con.Open();

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;
                        //Para el Redimensionamiento de los arrays de DIAS...en el Metodode Grilla Principal...
                        xCantOs = dt.Rows.Count;

                        if (dt.Rows.Count > 0)
                        {
                            xFcrea = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                //Right("000" & rs!RNOS, 6)
                                xFcrea = xFcrea + "{";
                                xFcrea = xFcrea + '"' + "IdCodfil" + '"' + ":" + '"' + dt.Rows[i]["CODFIL"].ToString() + '"' + ",";
                                xFcrea = xFcrea + '"' + "FilName" + '"' + ":" + '"' + dt.Rows[i]["CODFIL"].ToString() + " - " + dt.Rows[i]["DESCFIL"].ToString() + '"';
                                if (i < dt.Rows.Count - 1)
                                {
                                    xFcrea = xFcrea + "},";
                                }
                                else
                                {
                                    xFcrea = xFcrea + "}";
                                }

                            }
                            xFcrea = xFcrea + "]";
                        }
                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "FILIAL DE CREACION:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            xFcrea = "[]";
                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        xFcrea = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            xFcrea = xFcrea + "{";
                            xFcrea = xFcrea + '"' + "ERROR" + '"' + " FILIAL DE CREACION:" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                xFcrea = xFcrea + "}";
                            }
                            else
                            {
                                xFcrea = xFcrea + "},";
                            };
                        }
                        xFcrea = xFcrea + "]";
                    }
                    con.Close();
                    return xFcrea;
                }
            }
        }

        public string PopulateDropDownListFservicio(string xFserv, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            int xContaErr = 0;
            StatusWs objStatus = new StatusWs();
            DataTable dt = new DataTable();
            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM GRAL_FILIALES WITH(NOLOCK) WHERE ESTFIL = 'A' ORDER BY CODFIL", con))
                {
                    try
                    {
                        con.Open();

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;
                        //Para el Redimensionamiento de los arrays de DIAS...en el Metodode Grilla Principal...
                        xCantOs = dt.Rows.Count;

                        if (dt.Rows.Count > 0)
                        {
                            xFserv = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                //Right("000" & rs!RNOS, 6)
                                xFserv = xFserv + "{";
                                xFserv = xFserv + '"' + "IdCodfilServ" + '"' + ":" + '"' + dt.Rows[i]["CODFIL"].ToString() + '"' + ",";
                                xFserv = xFserv + '"' + "FilServName" + '"' + ":" + '"' + dt.Rows[i]["CODFIL"].ToString() + " - " + dt.Rows[i]["DESCFIL"].ToString() + '"';
                                if (i < dt.Rows.Count - 1)
                                {
                                    xFserv = xFserv + "},";
                                }
                                else
                                {
                                    xFserv = xFserv + "}";
                                }

                            }
                            xFserv = xFserv + "]";
                        }
                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "FILIAL DE SERVICIO:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            xFserv = "[]";
                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        xFserv = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            xFserv = xFserv + "{";
                            xFserv = xFserv + '"' + "ERROR" + '"' + " FILIAL DE SERVICIO:" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                xFserv = xFserv + "}";
                            }
                            else
                            {
                                xFserv = xFserv + "},";
                            };
                        }
                        xFserv = xFserv + "]";
                    }
                    con.Close();
                    return xFserv;
                }
            }
        }

        public string PopulateDropDownListUbicCont(string xUcont, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            DataTable dt = new DataTable();
            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);

            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM GRAL_FILIALES WITH(NOLOCK) WHERE CODFIL <> '' AND ESTFIL = 'A' ORDER BY CODFIL", con))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            xUcont = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                xUcont = xUcont + "{";
                                xUcont = xUcont + '"' + "IdUbicCont" + '"' + ":" + '"' + dt.Rows[i]["CODFIL"].ToString() + '"' + ",";
                                xUcont = xUcont + '"' + "UbicContName" + '"' + ":" + '"' + dt.Rows[i]["CODFIL"].ToString() + " - " + dt.Rows[i]["DESCFIL"].ToString() + '"' + "}";
                                if (i < (dt.Rows.Count - 1))
                                {
                                    xUcont = xUcont + ",";
                                }
                                
                            }
                        }
                        else
                        {
                            xUcont = "[";
                        }
                    }
                    catch (SqlException e)
                    {
                        xUcont = "[]";
                        con.Close();
                        return xUcont;
                    }

                    con.Close();

                    if (xUcont == "[")
                    {
                        xUcont = xUcont + "{";
                    }
                    else { 
                        xUcont = xUcont + ",{";
                    }

                    xUcont = xUcont + '"' + "IdUbicCont" + '"' + ":" + '"' + "61" + '"' + ",";
                    xUcont = xUcont + '"' + "UbicContName" + '"' + ":" + '"' + "61" + " - " + " Sede Central" + '"';
                    xUcont = xUcont + "}";

                    //Agrego Obras Sociales
                    using (SqlCommand cmd2 = new SqlCommand("SELECT * FROM GRAL_OBRASSOCIALES WITH(NOLOCK) WHERE CODOS IS NOT NULL ORDER BY CODOS", con))
                    {
                        try
                        {
                            con.Open();
                            SqlDataAdapter da = new SqlDataAdapter(cmd2);
                            dt = new DataTable();
                            da.Fill(dt);

                            int Contador = dt.Rows.Count;
                            xCantOs = dt.Rows.Count;

                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    xUcont = xUcont + ",{";
                                    xUcont = xUcont + '"' + "IdUbicCont" + '"' + ":" + '"' + dt.Rows[i]["CODOS"].ToString() + '"' + ",";
                                    if (dt.Rows[i]["CODOS"].ToString().Length < 2)
                                    {
                                        xUcont = xUcont + '"' + "UbicContName" + '"' + ":" + '"' + "00" + dt.Rows[i]["CODOS"].ToString() + " - " + dt.Rows[i]["DESCOS"].ToString() + '"' + "}";
                                    }else{
                                        xUcont = xUcont + '"' + "UbicContName" + '"' + ":" + '"' + dt.Rows[i]["CODOS"].ToString() + " - " + dt.Rows[i]["DESCOS"].ToString() + '"' + "}";
                                    }
                                    
                                }
                                xUcont = xUcont + "]";
                            }
                            else
                            {
                                xUcont = "]";
                            }
                        }
                        catch (SqlException e)
                        {
                            xUcont = "[]";
                        }
                    }
                    con.Close();
                    return xUcont;
                }
            }
        }

        public string PopulateDropDownListUbicAdm(string xUadm, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            int xContaErr = 0;
            StatusWs objStatus = new StatusWs();
            DataTable dt = new DataTable();
            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM GRAL_FILIALES WITH(NOLOCK) WHERE CODFIL <> '' AND ESTFIL = 'A' ORDER BY CODFIL", con))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            xUadm = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                xUadm = xUadm + "{";
                                xUadm = xUadm + '"' + "IdUbicCont" + '"' + ":" + '"' + dt.Rows[i]["CODFIL"].ToString() + '"' + ",";
                                xUadm = xUadm + '"' + "UbicContName" + '"' + ":" + '"' + dt.Rows[i]["CODFIL"].ToString() + " - " + dt.Rows[i]["DESCFIL"].ToString() + '"';

                                if (i < (dt.Rows.Count - 1)) { 
                                    xUadm = xUadm + "},";
                                }
                                else
                                {
                                    xUadm = xUadm + "}";
                                }
                            }
                        }
                        else
                        {
                            xUadm = "[";
                        }
                    }
                    catch (SqlException e)
                    {
                        xUadm = "[]";
                        con.Close();
                        return xUadm;
                    }
                    con.Close();

                    if (xUadm == "[")
                    {
                        xUadm = xUadm + "{";
                    }
                    else
                    {
                        xUadm = xUadm + ",{";
                    }

                    xUadm = xUadm + '"' + "IdUbicCont" + '"' + ":" + '"' + "61" + '"' + ",";
                    xUadm = xUadm + '"' + "UbicContName" + '"' + ":" + '"' + "61" + " - " + " Sede Central" + '"';
                    xUadm = xUadm + "}";

                    //Agrego Obras Sociales
                    using (SqlCommand cmd2 = new SqlCommand("SELECT * FROM GRAL_OBRASSOCIALES WITH(NOLOCK) WHERE CODOS IS NOT NULL ORDER BY CODOS", con))
                    {
                        try
                        {
                            con.Open();
                            SqlDataAdapter da = new SqlDataAdapter(cmd2);
                            dt = new DataTable();
                            da.Fill(dt);

                            int Contador = dt.Rows.Count;
                            xCantOs = dt.Rows.Count;

                            if (dt.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    xUadm = xUadm + ",{";
                                    xUadm = xUadm + '"' + "IdUbicCont" + '"' + ":" + '"' + dt.Rows[i]["CODOS"].ToString() + '"' + ",";
                                    if (dt.Rows[i]["CODOS"].ToString().Length < 2)
                                    {
                                        xUadm = xUadm + '"' + "UbicContName" + '"' + ":" + '"' + "00" + dt.Rows[i]["CODOS"].ToString() + " - " + dt.Rows[i]["DESCOS"].ToString() + '"' + "}";
                                    }
                                    else
                                    {
                                        xUadm = xUadm + '"' + "UbicContName" + '"' + ":" + '"' + dt.Rows[i]["CODOS"].ToString() + " - " + dt.Rows[i]["DESCOS"].ToString() + '"' + "}";
                                    }
                                    
                                }
                                xUadm = xUadm + "]";
                            }
                            else
                            {
                                xUadm = xUadm + "]";
                            }
                        }
                        catch (SqlException e)
                        {
                            xUadm = "[]";
                        }
                    }
                    con.Close();
                    return xUadm;
                }
            }
        }

        // METODO PARA LEER EL DATOS de CONCEPTOS DESDE EL HTML Y CARGAR EL COMBO DE FILIALES
        public string PopulateDropDownListFiliales(string xFil, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            int xContaErr = 0;
            StatusWs objStatus = new StatusWs();
            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT CODFIL, DESCFIL FROM GRAL_FILIALES WITH(NOLOCK) WHERE ESTFIL = 'A' ORDER BY CODFIL", con))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            xFil = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                xFil = xFil + "{";
                                xFil = xFil + '"' + "IdFil" + '"' + ":" + '"' + dt.Rows[i]["CODFIL"].ToString() + '"' + ",";
                                xFil = xFil + '"' + "FilName" + '"' + ":" + '"' + dt.Rows[i]["DESCFIL"].ToString() + '"' + "}";
                                if (i < (Contador - 1))
                                {
                                    xFil = xFil + ",";
                                }
                            }
                            xFil = xFil + "]";
                        }
                        else
                        {
                            xFil = "[]";
                        }
                    }
                    catch (SqlException e)
                    {
                        xFil = "[]";
                    }

                    con.Close();
                    return xFil;
                }
            }
        }
        public string PopulateDropDownListGrillaPrincipal(string xAgrup, string xCpto, string xOS, string xEstAdm, string xMesVto,
            string xAnioVto, string xEstCont, string xEstVto, string xAfil, string xAnioPrest,
            string xFechaSalida, string xFilialCreacion, string xFilialServicio, string xFilialContable, string xFilialUbicAdmin,
            string xNroAfiliado, string xNroSolicitudSur, string xExpedientes, string xNroId, string xFecreDesde, string xFecreHasta,
                         string xOrden, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            string xFilialLogin = xFilialLog;
            string xFILAFIL, xNROAFIL, xBENEFAFIL, xFEDESDE, xFEHASTA;
            string xFECRE, xFECHAESTADOADM, xFECHAESTADOCBLE;
            string flagConcepto;
            bool booSolInfoNotif, booRevSinGestion, booRevSinEmpadronar, booRevBaja;
            bool booPermiteEnvio, booPermiteBaja, booPermiteDevolver, booPermiteNoGestion, booPermiteEnvioRespNotif;
            bool booRespNotif;
            bool booDeshacerAUDITADO, booPermiteAuditLegAdm;
            bool booPermiteSinEmp, booPermiteDerivar, booDoc_DisponibleSC, booDoc_NODisponibleSC;
            booPermiteBaja = true;
            booPermiteEnvio = true;
            booPermiteAuditLegAdm = true;
            booRespNotif = false;
            booPermiteNoGestion = true;
            booPermiteSinEmp = true;
            booPermiteDerivar = true;
            booDoc_DisponibleSC = true;
            booDoc_NODisponibleSC = true;
            booDeshacerAUDITADO = true;
            booPermiteEnvioRespNotif = true;
            booRevBaja = false;
            booRevSinGestion = true;
            booRevSinEmpadronar = false;
            booPermiteDevolver = false;
            booSolInfoNotif = false;
            flagConcepto = "";
            int[] OSoc = new int[9];

            var xGPJ = new StringBuilder();
            var loqui = new StringBuilder();
            var mensaje = new StringBuilder();

            int xContOS = 0;
            int xContaErr = 0;
            string sSql;
            string xUcont = "";
            StatusWs objStatus = new StatusWs();

            if (xFecreDesde != "" && xFecreHasta != "")
            {
                try
                {
                    DateTime hasta = DateTime.Parse(xFecreHasta);
                    if (DateTime.Parse(xFecreDesde) > hasta)
                    {
                        return "La fecha Desde no puede ser MAYOR a fecha Hasta.";
                    }

                    hasta = hasta.AddDays(1);
                    xFecreHasta = hasta.ToString();
                }
                catch (Exception e)
                {
                    return "Las Fechas informadas son Incorrectas - " + e.Message +".";
                }
            }
            else if (xFecreDesde != "")
            {
                try
                {
                    DateTime.Parse(xFecreDesde);
                }
                catch (Exception e)
                {
                    return "Las Fechas Desde es Incorrecta - " + e.Message + ".";
                }
            }
            else if (xFecreHasta != "")
            {
                try
                {
                    DateTime hasta = DateTime.Parse(xFecreHasta);
                    hasta = hasta.AddDays(1);
                    xFecreHasta = hasta.ToString();
                }
                catch (Exception e)
                {
                    return "Las Fechas Hasta es Incorrecta - " + e.Message + ".";
                }
            }

            if (xFechaSalida != "")
            {
                try
                {
                    DateTime fechaSalida = DateTime.Parse(xFechaSalida);
                }
                catch (Exception e)
                {
                    return "La Fecha de salida es Incorrecta - " + e.Message + ".";
                }
            }

            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                mensaje.Append(xMensaje);
                return mensaje.ToString();
            }

            using (SqlConnection con1 = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT CODOS, DESCOS FROM GRAL_OBRASSOCIALES WITH(NOLOCK) ORDER BY CODOS", con1))
                {
                    try
                    {
                        con1.Open();
                        DataTable dt1 = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt1);
                        Array.Resize(ref OSoc, dt1.Rows.Count);
                        xContOS = dt1.Rows.Count;

                        if (dt1.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt1.Rows.Count; i++)
                            {
                                OSoc[i] = int.Parse(dt1.Rows[i]["CODOS"].ToString());
                            }
                        }
                    }

                    catch (SqlException ex)
                    {
                        string xMesgError;
                        xUcont = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            xUcont = xUcont + "{";
                            xUcont = xUcont + '"' + "ERROR" + '"' + " Array OS:" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                xUcont = xUcont + "}";
                            }
                            else
                            {
                                xUcont = xUcont + "},";
                            };
                        }
                        xUcont = xUcont + "]";
                        con.Close();
                        mensaje.Append(xUcont);
                        return mensaje.ToString();
                    }
                    con1.Close();
                }
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT CODOS,CONCEPTO,VALOR FROM TablasGrales WITH(NOLOCK)", con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt2 = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt2);

                        if (dt2.Rows.Count > 0)
                        {
                            for (int i = 0; i < xContOS; i++)
                            {

                                if (dt2.Rows[i]["CONCEPTO"].ToString() == "DIASROJO" && dt2.Rows[i]["CODOS"].ToString() == OSoc[i].ToString())
                                {
                                    DiasRojo[i] = int.Parse(dt2.Rows[i]["VALOR"].ToString());
                                }

                                if (dt2.Rows[i]["CONCEPTO"].ToString() == "DIASNARANJA" && dt2.Rows[i]["CODOS"].ToString() == OSoc[i].ToString())
                                {
                                    DiasNaranja[i] = int.Parse(dt2.Rows[i]["VALOR"].ToString());
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        xUcont = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            xUcont = xUcont + "{";
                            xUcont = xUcont + '"' + "ERROR" + '"' + " Array DIAS:" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                xUcont = xUcont + "}";
                            }
                            else
                            {
                                xUcont = xUcont + "},";
                            };
                        }
                        xUcont = xUcont + "]";
                        con.Close();

                        mensaje.Append(xUcont);

                        return mensaje.ToString();

                    }
                }


                //====================================================================================================================

                using (SqlConnection con2 = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
                {


                    sSql = "SELECT HAYCOMENTARIOS,SUM(TOTAL) AS TOTAL, COUNT(TOTAL) AS CANTRENG, MAX(FEVTO) AS FEVTO, NOMBREAFIL,";
                    sSql = sSql + "ID,FILAFIL,NROAFIL,BENEFAFIL,FEDESDE,FEHASTA,ESTADOADM,ESTADOCBLE,CODOS,CODCONC,UBICADM,FILDEBITO,";
                    sSql = sSql + "NROEXPDTE,RENOS,OCULTO,DOCA_DISPONIBLE,ID_ENTE_GOB,FECRE,FECHAESTADOADM,FECHAESTADOCBLE";
                    sSql = sSql + " FROM  (SELECT CABECERAS.NOMBREAFIL,CABECERAS.ID,CABECERAS.FILAFIL,CABECERAS.NROAFIL,CABECERAS.BENEFAFIL,CABECERAS.FEDESDE,";
                    sSql = sSql + " CABECERAS.FEHASTA,CABECERAS.ESTADOADM,CABECERAS.ESTADOCBLE,CODOS,GRAL_CONCEPTOS.PATOL_CODIF AS CODCONC,CABECERAS.UBICADM,";
                    sSql = sSql + " DATEADD(d, -1, DATEADD(m, DATEDIFF(m, 0, DATEADD(m, 22, Max(FEPREST))) + 1, 0)) AS FEVTO,";
                    sSql = sSql + " FILDEBITO,IMPORTE AS TOTAL,NROEXPDTE, CASE WHEN RNOS IS NULL THEN 'S/INFO' ELSE RNOS END AS RENOS,";
                    sSql = sSql + " CABECERAS.CODCONC AS OCULTO,DOCA_DISPONIBLE,cab_comentarios.idcab AS HAYCOMENTARIOS,ID_ENTE_GOB,CABECERAS.FECRE,";
                    sSql = sSql + " (SELECT MAX(FECRE) FROM CAB_SEGUIMIENTO CS WHERE CS.TIPO = 'ADM' AND CS.ESTADOANT <> CS.ESTADONUE";
                    sSql = sSql + " AND CS.IDCAB = CABECERAS.ID ) AS FECHAESTADOADM,(SELECT MAX(FECRE) FROM CAB_SEGUIMIENTO CS WHERE CS.TIPO = 'CBLE'";
                    sSql = sSql + " AND CS.ESTADOANT <> CS.ESTADONUE AND CS.IDCAB = CABECERAS.ID ) AS FECHAESTADOCBLE FROM (RENGLONES WITH(NOLOCK)";
                    sSql = sSql + " RIGHT JOIN (CABECERAS WITH(NOLOCK)";
                    sSql = sSql + " LEFT JOIN GRAL_AFILIADOS_SSS WITH(NOLOCK) ON CABECERAS.FILAFIL = GRAL_AFILIADOS_SSS.FILAFIL";
                    sSql = sSql + " AND CABECERAS.NROAFIL = GRAL_AFILIADOS_SSS.NROAFIL";
                    sSql = sSql + " AND CABECERAS.BENEFAFIL = GRAL_AFILIADOS_SSS.BENEFAFIL";
                    sSql = sSql + " AND CABECERAS.FEDESDE >= GRAL_AFILIADOS_SSS.FEDESDE";
                    sSql = sSql + " AND CABECERAS.FEDESDE < CASE WHEN GRAL_AFILIADOS_SSS.FEHASTA IS NULL THEN '20991231'";
                    sSql = sSql + " ELSE GRAL_AFILIADOS_SSS.FEHASTA END) ON CABECERAS.ID = RENGLONES.IDCAB)";
                    sSql = sSql + " JOIN gral_conceptos WITH(NOLOCK) ON cabeceras.codconc=gral_conceptos.codconc";
                    sSql = sSql + " LEFT JOIN cab_comentarios WITH(NOLOCK) ON cabeceras.id = cab_comentarios.idcab";
                    sSql = sSql + " WHERE CABECERAS.ID>0 ";

                    //6 Nros. Expedientes
                    if (xExpedientes != null && xExpedientes != "" && xExpedientes != "?")
                    {
                        sSql = sSql + " AND CABECERAS.NROEXPDTE IN  (" + xExpedientes + ")";
                    }
                    //5 IDs
                    if (xNroId != null && xNroId != "" && xNroId != "?")
                    {
                        sSql = sSql + " AND CABECERAS.ID IN (" + xNroId + ")";
                    }
                    //7 Nros. Solicitud Sur
                    if (xNroSolicitudSur != null && xNroSolicitudSur != "" && xNroSolicitudSur != "?")
                    {
                        sSql = sSql + " AND CABECERAS.ID_ENTE_GOB IN  (" + xNroSolicitudSur + ")";
                    }
                    //8 Nros. AFILIADOS
                    if (xNroAfiliado != null && xNroAfiliado != "" && xNroAfiliado != "?")
                    {
                        sSql = sSql + " AND RIGHT('00' + cast(CABECERAS.FILAFIL as varchar),2) + RIGHT('0000000' + cast(CABECERAS.NROAFIL as varchar),7) + RIGHT('00' + cast(CABECERAS.BENEFAFIL as varchar),2) IN  (" + xNroAfiliado + ")";
                    }
                    //1 AGRUPACION
                    if (xAgrup != null && xAgrup != "" && xAgrup != "?")
                    {
                        sSql = sSql + " AND CABECERAS.CODCONC = ANY (SELECT CODCONC FROM GRAL_CONCEPTOS WITH (NOLOCK) WHERE AGRUPCONC = " + xAgrup + ") ";
                    }
                    //2 CONCEPTOS
                    if (xCpto != null && xCpto != "" && xCpto != "?")
                    {
                        sSql = sSql + " AND CABECERAS.CODCONC IN (" + xCpto + ") ";
                    }
                    //3 OBRA SOCIAL
                    if (xOS != null && xOS != "" && xOS != "?")
                    {
                        sSql = sSql + " AND CABECERAS.CODOS IN (" + xOS + ") ";
                    }
                    //14 AÑO PRESTACION
                    if (xAnioPrest != null && xAnioPrest != "" && xAnioPrest != "?")
                    {
                        sSql = sSql + " AND Right(CONVERT(VARCHAR(10),CABECERAS.FEDESDE,103),4) IN (" + xAnioPrest + ") ";
                    }
                    //15 ESTADO ADMINISTRATIVO
                    if (xEstAdm != null && xEstAdm != "" && xEstAdm != "?")
                    {
                        sSql = sSql + " AND ESTADOADM IN (" + xEstAdm + ") ";
                    }
                    //15 ESTADO CONTABLE
                    if (xEstCont != null && xEstCont != "" && xEstCont != "?")
                    {
                        sSql = sSql + " AND ESTADOCBLE IN (" + xEstCont + ")";
                    }
                    //10 FILIAL CREACION
                    if (xFilialCreacion != null && xFilialCreacion != "" && xFilialCreacion != "?")
                    {
                        sSql = sSql + " AND LEFT(RIGHT('0000000000' + cast (CABECERAS.ID as NUMERIC),10),CASE WHEN LEN(CAST(CABECERAS.ID AS NUMERIC)) = '9' THEN 1 ELSE 2 END) IN (" + xFilialCreacion + ")";
                    }
                    //4 RNOS            AND
                    if (xAfil != null && xAfil != "" && xAfil != "?")
                    {
                        sSql = sSql + " AND RNOS IN (" + xAfil + ")";
                    }
                    //11 FILIAL DEBITO
                    if (xFilialServicio != null && xFilialServicio != "" && xFilialServicio != "?")
                    {
                        sSql = sSql + "  AND FILDEBITO IN  (" + xFilialServicio + ")";
                    }
                    //12 FILIAL UBICACION ADMINISTRATIVA
                    if (xFilialUbicAdmin != null && xFilialUbicAdmin != "" && xFilialUbicAdmin != "?")
                    {
                        sSql = sSql + "  AND UBICADM IN  (" + xFilialUbicAdmin + ")";
                    }
                    //13 FILIAL UBICACION CONTABLE
                    if (xFilialContable != null && xFilialContable != "" && xFilialContable != "?")
                    {
                        sSql = sSql + "  AND UBICCBLE IN  (" + xFilialContable + ")";
                    }
                    //9 FECHA DE SALIDA
                    if (xFechaSalida != null && xFechaSalida != "" && xFechaSalida != "?")
                    {
                       sSql = sSql + " AND FESALIDA='" + FormatoFecha(xFechaSalida, "yyyy/mm/dd", false) + "'";   
                    }
                    //16 Fecha de Creacion del registro de Cabecera (para achicar el rango de datos devueltos)
                    if (xFecreDesde != null && xFecreHasta != null && xFecreDesde != "" && xFecreHasta != "")
                    {
                        sSql = sSql + " AND CABECERAS.FECRE BETWEEN '" + FormatoFecha(xFecreDesde, "yyyy/mm/dd", true) + "' AND '" + FormatoFecha(xFecreHasta, "yyyy/mm/dd", true) + "'";
                    }
                    else if (xFecreDesde != null && xFecreDesde != "")
                    {
                        sSql = sSql + " AND CABECERAS.FECRE > '" + FormatoFecha(xFecreDesde, "yyyy/mm/dd", true) + "' ";
                    }
                    else if (xFecreHasta != null && xFecreHasta != "")
                    {
                        sSql = sSql + " AND CABECERAS.FECRE < '" + FormatoFecha(xFecreHasta, "yyyy/mm/dd", true) + "' ";
                    }

                    // FIN DE LOS FILTROS

                    sSql = sSql + " GROUP BY CABECERAS.NOMBREAFIL, CABECERAS.ID, CABECERAS.FILAFIL, CABECERAS.NROAFIL, CABECERAS.BENEFAFIL, ";
                    sSql = sSql + " CABECERAS.FEDESDE, CABECERAS.FEHASTA, CABECERAS.ESTADOADM, CABECERAS.ESTADOCBLE, CODOS, GRAL_CONCEPTOS.PATOL_CODIF, FILDEBITO, UBICADM, ";
                    sSql = sSql + " NROEXPDTE, CASE WHEN RNOS IS NULL then 'S/INFO' else RNOS END,CABECERAS.CODCONC,CAB_COMENTARIOS.IDCAB,DOCA_DISPONIBLE,";
                    sSql = sSql + "RENGLONES.IMPORTE , RENGLONES.ID, gral_conceptos.TIPO_VENCIMIENTO, ID_ENTE_GOB, CABECERAS.FECRE";

                    if (xMesVto != null && xMesVto != "" && xMesVto != "?")
                    {

                        string s1 = sSql;
                        string s2 = "HAVING";
                        bool b = s1.Contains(s2);
                        if (b == false)
                        {
                            sSql = sSql + " HAVING ";

                        }
                        else
                        {
                            sSql = sSql + " AND ";
                        }
                        sSql = sSql + " CASE GRAL_CONCEPTOS.TIPO_VENCIMIENTO WHEN 'P' then MONTH(MAX(FEPREST + 120)) else MONTH(MAX(FERC + 90)) END IN (" + xMesVto + ")";
                    }

                    if (xAnioVto != null && xAnioVto != "" && xAnioVto != "?")
                    {

                        string s3 = sSql;
                        string s4 = "HAVING";
                        bool bb = s3.Contains(s4);
                        if (bb == false)
                        {
                            sSql = sSql + " HAVING ";

                        }
                        else
                        {
                            sSql = sSql + " AND ";
                        }
                        sSql = sSql + " RIGHT(CASE GRAL_CONCEPTOS.TIPO_VENCIMIENTO WHEN 'P' then YEAR(MAX(FEPREST + 120)) else YEAR(MAX(FERC + 90)) END,4) IN (" + xAnioVto + ")";
                    }

                    sSql = sSql + ") AS TEMPORAL";

                    sSql = sSql + " GROUP BY HAYCOMENTARIOS, NOMBREAFIL, ID, FILAFIL, NROAFIL, BENEFAFIL, FEDESDE, FEHASTA, UBICADM, ESTADOADM, ESTADOCBLE, CODOS,";
                    sSql = sSql + " CODCONC, OCULTO, FILDEBITO, NROEXPDTE, RENOS,DOCA_DISPONIBLE, ID_ENTE_GOB,FECRE, FECHAESTADOADM, FECHAESTADOCBLE ";
                    if (xOrden != null && xOrden != "" && xOrden != "?")
                    {
                        xOrden = xOrden.Substring(1, xOrden.Length - 2);
                        sSql = sSql + " ORDER BY  " + xOrden;
                    }
                    else
                    {
                        sSql = sSql + " ORDER BY NOMBREAFIL ASC";
                    }

                    using (SqlCommand cmd = new SqlCommand(sSql, con2))
                    {

                        try
                        {
                            con2.Open();
                            DataTable dt = new DataTable();
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.SelectCommand.CommandTimeout = 600;
                            da.Fill(dt);
                            int Contador = dt.Rows.Count;
                            decimal xPaginas = 0;
                            decimal xContador = Contador;
                            if (dt.Rows.Count > 0)
                            {
                                xGPJ.Append("[");
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    int valorIco = 0;
                                    Boolean booEliminar = false;

                                    string DiaVto = "0";
                                    string xIcono = "";

                                    if (dt.Rows[i]["FEVTO"].ToString() != null && dt.Rows[i]["FEVTO"].ToString() != "")
                                    {
                                        DiaVto = string.Format("{0:d}", dt.Rows[i]["FEVTO"].ToString()); //'DD/MM/YYYY'
                                    }
                                    else
                                    {
                                        DiaVto = "0";
                                    }

                                    Array.Resize(ref DiasRojo, xContOS);
                                    Array.Resize(ref DiasNaranja, xContOS);


                                    for (int j = 0; j < xContOS; j++)
                                    {
                                        if (OSoc[j] == Int32.Parse(dt.Rows[i]["CODOS"].ToString()))
                                        {
                                            if (DiaVto == "0")
                                            {
                                                xIcono = "";
                                                break;
                                            }
                                            else //Debe ser una fecha...
                                            {
                                                if (Convert.ToDateTime(DiaVto) > DateTime.Now.AddDays(DiasNaranja[j]))
                                                {
                                                    xIcono = "<" + "img src='pages/template/img/icono_2.png' title='En Curso'" + ">"; //"2"; // VERDE
                                                    valorIco = 2;
                                                    break;

                                                }
                                                if (Convert.ToDateTime(DiaVto) > DateTime.Now.AddDays(DiasRojo[j]))
                                                {
                                                    xIcono = "<" + "img src='pages/template/img/icono_3.png' title='Por Vencer'" + ">"; //"3"; // NARANJA
                                                    valorIco = 3;
                                                    break;
                                                }
                                                if (Convert.ToDateTime(DiaVto) < DateTime.Now)
                                                {
                                                    xIcono = "<" + "img src='pages/template/img/icono_5.png' title='Vencido'" + ">"; //"5"; //VENCIDO
                                                    valorIco = 5;
                                                    break;
                                                }
                                                if (Convert.ToDateTime(DiaVto) <= DateTime.Now.AddDays(DiasRojo[j]))
                                                {
                                                    xIcono = "<" + "img src='pages/template/img/icono_4.png' title='Por Vencer'" + ">"; //"4"; //ROJO
                                                    valorIco = 4;
                                                    break;
                                                }
                                            }

                                        }
                                    }

                                    // AND
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "FINALIZADO" && DiaVto != "0")
                                    {
                                        xIcono = "<" + "img src='pages/template/img/icono_8.png' title='Notificado'" + ">"; //"8";
                                        valorIco = 8;
                                    }
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "BAJA" && DiaVto != "0")
                                    {
                                        xIcono = "<" + "img src='pages/template/img/icono_7.png' title='Baja'" + ">"; //"7";
                                        valorIco = 7;
                                    }
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "FINALIZADO" && DiaVto == "0")
                                    {
                                        xIcono = "<" + "img src='pages/template/img/icono_11.png' title='Notificado'" + ">"; //"11";
                                        valorIco = 11;
                                    }
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "BAJA" && DiaVto == "0")
                                    {
                                        xIcono = "<" + "img src='pages/template/img/icono_9.png' title='Baja'" + ">"; //"9";
                                        valorIco = 9;
                                    }


                                    if (xEstVto != "")
                                    {
                                        int tipoVto = 0;
                                        switch (xEstVto.ToUpper())
                                        {
                                            case "'ROJO'":
                                                tipoVto = 4;
                                                break;
                                            case "'NARANJA'":
                                                  tipoVto = 3;
                                                break;
                                            case "'VERDE'":
                                                  tipoVto = 2;
                                                break;
                                            case "'VENCIDO'":
                                                tipoVto = 5;
                                                break;
                                            case "'SIN VENCIMIENTO'":
                                                tipoVto = 0;
                                                break;
                                            default:
                                                tipoVto = 0;
                                                break;
                                        }

                                        if (tipoVto == 0 && (valorIco == 0 || valorIco == 9 || valorIco == 11))
                                        {
                                            booEliminar = false;
                                        }
                                        else if (tipoVto == 0 && (valorIco != 0 && valorIco != 9 && valorIco != 11))
                                        {
                                            booEliminar = true;
                                        }
                                        else if (tipoVto != 0)
                                        {
                                            if (valorIco != tipoVto)
                                            {
                                                booEliminar = true;
                                            }
                                            else
                                            {
                                                booEliminar = false;
                                            }
                                        }

                                    }

                                    if (booEliminar)
                                    {
                                        continue;
                                    }

                                    if (xGPJ.ToString().Length > 1)
                                    {
                                        xGPJ.Append(",{");
                                    }
                                    else
                                    {
                                        xGPJ.Append("{");
                                    }

                                    loqui.Append(dt.Rows[i]["ID"].ToString()+ "||");
                                    xGPJ.Append('"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["ID"].ToString() + '"' + ",");
                                    xGPJ.Append('"' + "NOMBREAFIL" + '"' + ":" + '"' + dt.Rows[i]["NOMBREAFIL"].ToString() + '"' + ",");

                                    xFILAFIL = dt.Rows[i]["FILAFIL"].ToString();
                                    xFILAFIL = xFILAFIL.ToString().PadLeft(2, '0');
                                    xNROAFIL = dt.Rows[i]["NROAFIL"].ToString();
                                    xNROAFIL = xNROAFIL.ToString().PadLeft(7, '0');
                                    xBENEFAFIL = dt.Rows[i]["BENEFAFIL"].ToString();
                                    xBENEFAFIL = xBENEFAFIL.ToString().PadLeft(2, '0');

                                    xGPJ.Append('"' + "NROAFIL" + '"' + ":" + '"' + xFILAFIL + "-" + xNROAFIL + "-" + xBENEFAFIL + '"' + ",");

                                    xFEDESDE = FormatoFecha(dt.Rows[i]["FEDESDE"].ToString(), "dd/mm/yyyy", false); //Convert.ToDateTime(dt.Rows[i]["FEDESDE"]).ToString("dd/MM/yyyy");

                                    xFEHASTA = FormatoFecha(dt.Rows[i]["FEHASTA"].ToString(), "dd/mm/yyyy", false); //Convert.ToDateTime(dt.Rows[i]["FEHASTA"]).ToString("dd/MM/yyyy");

                                    xGPJ.Append('"' + "PERIODO" + '"' + ":" + '"' + xFEDESDE + "-" + xFEHASTA + '"' + ",");
                                    xGPJ.Append('"' + "CODOS" + '"' + ":" + '"' + dt.Rows[i]["CODOS"].ToString() + '"' + ",");
                                    xGPJ.Append('"' + "CODCONC" + '"' + ":" + '"' + dt.Rows[i]["CODCONC"].ToString() + '"' + ",");
                                    xGPJ.Append('"' + "FEVTO" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FEVTO"].ToString(), "dd/mm/yyyy", false) + '"' + ",");
                                    xGPJ.Append('"' + "CANTRENG" + '"' + ":" + '"' + dt.Rows[i]["CANTRENG"].ToString() + '"' + ",");
                                    xGPJ.Append('"' + "TOTAL" + '"' + ":" + '"' + dt.Rows[i]["TOTAL"].ToString() + '"' + ",");
                                    xGPJ.Append('"' + "FILDEBITO" + '"' + ":" + '"' + dt.Rows[i]["FILDEBITO"].ToString() + '"' + ",");
                                    xGPJ.Append('"' + "ESTADOADM" + '"' + ":" + '"' + dt.Rows[i]["ESTADOADM"].ToString() + '"' + ",");
                                    xGPJ.Append('"' + "ESTADOCBLE" + '"' + ":" + '"' + dt.Rows[i]["ESTADOCBLE"].ToString() + '"' + ",");
                                    xGPJ.Append('"' + "ICONO" + '"' + ":" + '"' + xIcono + '"' + ",");

                                    if (dt.Rows[i]["HAYCOMENTARIOS"].ToString() != "")
                                    {
                                        xGPJ.Append('"' + "icoComen" + '"' + ":" + '"' + "<" + "img src='pages/template/img/icono_6.png' title='Comentario'" + ">" + '"' + ",");
                                    }
                                    else
                                    {
                                        xGPJ.Append('"' + "icoComen" + '"' + ":" + '"' + '"' + ",");
                                    }

                                    xGPJ.Append('"' + "HAYCOMENTARIOS" + '"' + ":" + '"' + dt.Rows[i]["HAYCOMENTARIOS"].ToString() + '"' + ",");
                                    xGPJ.Append('"' + "UBICADM" + '"' + ":" + '"' + dt.Rows[i]["UBICADM"].ToString() + '"' + ",");
                                    xGPJ.Append('"' + "NROEXPDTE" + '"' + ":" + '"' + dt.Rows[i]["NROEXPDTE"].ToString() + '"' + ",");

                                    xGPJ.Append('"' + "RENOS" + '"' + ":" + '"' + dt.Rows[i]["RENOS"].ToString() + '"' + ",");

                                    xGPJ.Append('"' + "OCULTO" + '"' + ":" + '"' + dt.Rows[i]["OCULTO"].ToString() + '"' + ",");

                                    if (dt.Rows[i]["DOCA_DISPONIBLE"].ToString() != "")
                                    {
                                        xGPJ.Append('"' + "DOCA_DISPONIBLE" + '"' + ":" + '"' + "<img src='pages/template/img/icono_11.png' title='Notificado'>" + '"' + ",");
                                    }
                                    else
                                    {
                                        xGPJ.Append('"' + "DOCA_DISPONIBLE" + '"' + ":" + '"' + '"' + ",");
                                    }

                                    xGPJ.Append('"' + "ID_ENTE_GOB" + '"' + ":" + '"' + dt.Rows[i]["ID_ENTE_GOB"].ToString() + '"' + ",");

                                    if (dt.Rows[i]["FECRE"].ToString() != null && dt.Rows[i]["FECRE"].ToString() != "")
                                    {
                                        xFECRE = FormatoFecha(dt.Rows[i]["FECRE"].ToString(), "dd/mm/yyyy", false);
                                    }
                                    else
                                    { 
                                        xFECRE = ""; 
                                    }
                                    xGPJ.Append('"' + "FECRE" + '"' + ":" + '"' + xFECRE + '"' + ",");

                                    if (dt.Rows[i]["FECHAESTADOADM"].ToString() != null && dt.Rows[i]["FECHAESTADOADM"].ToString() != "")
                                    {
                                        xFECHAESTADOADM = FormatoFecha(dt.Rows[i]["FECHAESTADOADM"].ToString(), "dd/mm/yyyy", false);
                                        //xFECHAESTADOADM = Convert.ToDateTime(dt.Rows[i]["FECHAESTADOADM"]).ToString("dd/MM/yyyy");
                                    }
                                    else
                                    { xFECHAESTADOADM = ""; }

                                    xGPJ.Append('"' + "FECHAESTADOADM" + '"' + ":" + '"' + xFECHAESTADOADM + '"' + ",");

                                    if (dt.Rows[i]["FECHAESTADOCBLE"].ToString() != null && dt.Rows[i]["FECHAESTADOCBLE"].ToString() != "")
                                    {
                                        //xFECHAESTADOCBLE = Convert.ToDateTime(dt.Rows[i]["FECHAESTADOCBLE"]).ToString("dd/MM/yyyy");
                                        xFECHAESTADOCBLE = FormatoFecha(dt.Rows[i]["FECHAESTADOCBLE"].ToString(), "dd/mm/yyyy", false);
                                    }
                                    else
                                    { 
                                        xFECHAESTADOCBLE = ""; 
                                    }

                                    xGPJ.Append('"' + "FECHAESTADOCBLE" + '"' + ":" + '"' + xFECHAESTADOCBLE + '"' + ",");

                                    //Flag's para habilitar acciones  --tony
                                    if (dt.Rows[i]["UBICADM"].ToString() != xFilialLogin || int.Parse(xFilialLogin) >= 61)
                                    { 
                                        booPermiteDerivar = false; 
                                    }
                                    else
                                    { 
                                        booPermiteDerivar = true; 
                                    }

                                    if (dt.Rows[i]["UBICADM"].ToString() != xFilialLogin || int.Parse(xFilialLogin) == 61 || int.Parse(xFilialLogin) > 100)
                                    {
                                        booPermiteEnvio = false;
                                        booPermiteEnvioRespNotif = false;
                                    }
                                    else
                                    {
                                        booPermiteEnvio = true;
                                        booPermiteEnvioRespNotif = true;
                                    }
                                    if (dt.Rows[i]["UBICADM"].ToString() != xFilialLogin || int.Parse(xFilialLogin) != 61)
                                    {
                                        booDoc_DisponibleSC = false;
                                        booDoc_NODisponibleSC = false;
                                        booDeshacerAUDITADO = false;
                                    }
                                    else
                                    {
                                        if (dt.Rows[i]["DOCA_DISPONIBLE"].ToString() == "S")
                                        {
                                            booDoc_DisponibleSC = false;
                                        }
                                        else
                                        {
                                            if (dt.Rows[i]["DOCA_DISPONIBLE"].ToString() == null && booDoc_NODisponibleSC == true)
                                            {
                                                booDoc_NODisponibleSC = false;
                                            }
                                        }
                                    }
                                    //--------------------------------------------------------------------------------------------
                                    if (int.Parse(xFilialLogin) == 61 || int.Parse(xFilialLogin) > 100)
                                    {
                                        // SI SE CUMPLE.....
                                        // A
                                        if ((dt.Rows[i]["ESTADOADM"].ToString() == "NOTIFICADO" &&
                                            int.Parse(dt.Rows[i]["UBICADM"].ToString()) > 60) ||
                                            (dt.Rows[i]["ESTADOADM"].ToString() == "NOTIFICADO" &&
                                            int.Parse(dt.Rows[i]["UBICADM"].ToString()) == 0))
                                        {
                                            booSolInfoNotif = true;
                                        }
                                        else // B
                                        {

                                            if ((dt.Rows[i]["ESTADOADM"].ToString() == "NOTIFICADO" &&
                                                 dt.Rows[i]["UBICADM"].ToString() == xFilialLogin) ||
                                                (dt.Rows[i]["ESTADOADM"].ToString() == "NOTIFICADO" &&
                                                int.Parse(dt.Rows[i]["UBICADM"].ToString()) == 0))
                                            {
                                                //booPermiteEnvioRespNotif = true; doc del 03/04/2018
                                                booRespNotif = true;
                                            }
                                            else // C
                                            {
                                                if (dt.Rows[i]["ESTADOADM"].ToString() != "FINALIZADO" &&
                                                   dt.Rows[i]["UBICADM"].ToString() != xFilialLogin)
                                                {
                                                    booPermiteAuditLegAdm = false;
                                                }
                                            }
                                        }
                                    }
                                    // SI NO SE CUMPLEN
                                    else
                                    {
                                        booPermiteAuditLegAdm = false;
                                    }

                                    //--------------------------------------------------------------------------------------------
                                    if (int.Parse(xFilialLogin) >= 100)
                                    {
                                        booPermiteBaja = false;
                                        booPermiteEnvio = false;
                                        booPermiteDevolver = false;
                                        booPermiteAuditLegAdm = false;
                                        booPermiteEnvioRespNotif = false;

                                    }
                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["UBICADM"].ToString() != xFilialLogin)
                                    {
                                        booPermiteBaja = false;
                                        booPermiteNoGestion = false;
                                        booPermiteSinEmp = false;
                                    }
                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["UBICADM"].ToString() == xFilialLogin && int.Parse(xFilialLogin) == 61)
                                    {
                                        booPermiteDevolver = true;
                                    }
                                    //--------------------------------------------------------------------------------------------
                                    if (int.Parse(dt.Rows[i]["UBICADM"].ToString()) > 61)
                                    {

                                        booPermiteBaja = false;
                                        booPermiteNoGestion = false;
                                        booPermiteSinEmp = false;

                                    }

                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "NOTIFICADO" &&
                                        dt.Rows[i]["ESTADOADM"].ToString() == "NO APE" &&
                                        dt.Rows[i]["ESTADOADM"].ToString() == "BAJA")
                                    {
                                        booPermiteBaja = false;
                                    }
                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "CERRADO" &&
                                        dt.Rows[i]["ESTADOADM"].ToString() == "SIN EMPADRONAR" &&
                                        dt.Rows[i]["ESTADOADM"].ToString() == "NOTIFICADO" &&
                                        dt.Rows[i]["ESTADOADM"].ToString() == "NO APE" &&
                                        dt.Rows[i]["ESTADOADM"].ToString() == "AUDITADO" &&
                                        dt.Rows[i]["ESTADOADM"].ToString() == "FINALIZADO" &&
                                        dt.Rows[i]["ESTADOADM"].ToString() == "BAJA")
                                    {
                                        booPermiteNoGestion = false;
                                    }

                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["ESTADOADM"].ToString() != "AUDITADO")
                                    {
                                        booDeshacerAUDITADO = false;
                                    }
                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["ESTADOADM"].ToString() != "EN GESTION")
                                    {
                                        booPermiteSinEmp = false;
                                    }

                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "FINALIZADO" &&
                                        dt.Rows[i]["ESTADOADM"].ToString() == "NOTIFICADO")
                                    {
                                        booPermiteEnvio = false;
                                    }

                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "NO APE")
                                    {
                                        booPermiteEnvio = false;
                                    }

                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["ESTADOADM"].ToString() != "NOTIFICADO")
                                    {
                                        booPermiteEnvioRespNotif = false;
                                    }

                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "BAJA" &&
                                        dt.Rows[i]["UBICADM"].ToString() == xFilialLogin)
                                    {
                                        booRevBaja = true;
                                    }
                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "CERRADO" &&
                                        dt.Rows[i]["UBICADM"].ToString() == xFilialLogin)
                                    {
                                        booRevSinGestion = true;
                                    }
                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["ESTADOADM"].ToString() == "SIN EMPADRONAR" &&
                                        dt.Rows[i]["UBICADM"].ToString() == xFilialLogin)
                                    {
                                        booRevSinEmpadronar = true;
                                    }
                                    //--------------------------------------------------------------------------------------------
                                    if (dt.Rows[i]["OCULTO"].ToString() == "DISCAPACIDAD")
                                    {
                                        flagConcepto = "Discapacidad";
                                    }
                                    else
                                    {
                                        flagConcepto = "Otro";
                                    }

                                    //=============================================================================================================
                                    xGPJ.Append('"' + "booPermiteBaja" + '"' + ":" + '"' + booPermiteBaja + '"' + ",");

                                    xGPJ.Append('"' + "booPermiteEnvio" + '"' + ":" + '"' + booPermiteEnvio + '"' + ",");

                                    xGPJ.Append('"' + "booPermiteAuditLegAdm" + '"' + ":" + '"' + booPermiteAuditLegAdm + '"' + ",");

                                    xGPJ.Append('"' + "booRespNotif" + '"' + ":" + '"' + booRespNotif + '"' + ",");

                                    xGPJ.Append('"' + "booPermiteNoGestion" + '"' + ":" + '"' + booPermiteNoGestion + '"' + ",");

                                    xGPJ.Append('"' + "booPermiteSinEmp" + '"' + ":" + '"' + booPermiteSinEmp + '"' + ",");

                                    xGPJ.Append('"' + "booPermiteDerivar" + '"' + ":" + '"' + booPermiteDerivar + '"' + ",");

                                    xGPJ.Append('"' + "booDoc_DisponibleSC" + '"' + ":" + '"' + booDoc_DisponibleSC + '"' + ",");
                                    //=============================================================================================================
                                    xGPJ.Append('"' + "booDoc_NODisponibleSC" + '"' + ":" + '"' + booDoc_NODisponibleSC + '"' + ",");

                                    xGPJ.Append('"' + "booDeshacerAuditado" + '"' + ":" + '"' + booDeshacerAUDITADO + '"' + ",");

                                    xGPJ.Append('"' + "booPermiteEnvioRespNotif" + '"' + ":" + '"' + booPermiteEnvioRespNotif + '"' + ",");

                                    xGPJ.Append('"' + "booRevBaja" + '"' + ":" + '"' + booRevBaja + '"' + ",");

                                    xGPJ.Append('"' + "booRevSinGestion" + '"' + ":" + '"' + booRevSinGestion + '"' + ",");

                                    xGPJ.Append('"' + "booRevSinEmpadronar" + '"' + ":" + '"' + booRevSinEmpadronar + '"' + ",");

                                    xGPJ.Append('"' + "booPermiteDevolver" + '"' + ":" + '"' + booPermiteDevolver + '"' + ",");

                                    xGPJ.Append('"' + "booSolInfoNotif" + '"' + ":" + '"' + booSolInfoNotif + '"' + ",");

                                    xGPJ.Append('"' + "flagConcepto" + '"' + ":" + '"' + flagConcepto + '"' + ",");

                                    // Datos de Paginación
                                    xGPJ.Append('"' + "CANTCABECERA" + '"' + ":" + '"' + Contador.ToString() + '"' + ",");
                                    xPaginas = xContador / 30;
                                    xGPJ.Append('"' + "NROPAG" + '"' + ":" + '"' + Math.Ceiling(xPaginas) + '"');
                                    xGPJ.Append("}");
                                }
                                xGPJ.Append("]");
                            }
                            else
                            {
                                xGPJ.Append("[]");
                            }
                        }
                        catch (SqlException ex)
                        {
                            xGPJ = new StringBuilder();
                            xGPJ.Append("SqlException[" + ex.Message +"||"+loqui +"]");
                        }
                        catch (Exception e)
                        {
                            xGPJ = new StringBuilder();
                            xGPJ.Append("Exception[" + e.Message +"||"+loqui + "]");
                        }

                        con.Close();

                        return xGPJ.ToString();
                    }
                }
            }
        }

        public string PopulateDropDownListGrillaSinCabecera(string xSC, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string xFilialParam = xSC;
            string sSql = "";
            int xContaErr = 0;
            string xFEPREST;
            string xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT CAST(ID as numeric) as id, RENGLONES.FILAFIL, RENGLONES.NROAFIL, RENGLONES.BENEFAFIL, ";
                sSql = sSql + " APELLIDO + ' ' + NOMBRE AS NOMBREAFIL, IMPORTE, UBIC, GRAL_CONCEPTOS.PATOL_CODIF AS CODCONC, ";
                sSql = sSql + " GRAL_CONCEPTOS.CODCONC AS OCULTO, FEPREST, FECRE FROM RENGLONES WITH(NOLOCK) LEFT JOIN GRAL_AFILIADOS ";
                sSql = sSql + " WITH(NOLOCK) ON RENGLONES.FILAFIL = GRAL_AFILIADOS.FILAFIL AND RENGLONES.NROAFIL = GRAL_AFILIADOS.NROAFIL ";
                sSql = sSql + " AND RENGLONES.BENEFAFIL = GRAL_AFILIADOS.BENEFAFIL JOIN GRAL_CONCEPTOS WITH(NOLOCK) ";
                sSql = sSql + " ON RENGLONES.CODCONC = GRAL_CONCEPTOS.CODCONC ";
                sSql = sSql + " WHERE IDCAB = 0 ";
                sSql = sSql + " AND LEFT(RIGHT('0000000000' + CAST(ID as numeric),10),CASE WHEN LEN(CAST(ID AS NUMERIC)) = '9' THEN 1 ";
                sSql = sSql + " ELSE 2 END) = " + xFilialParam;
                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    try
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        //xSC = "[{" + '"' + "Nroid" + '"' + ":" + '"' + "id" + '"' + "," + '"' + " NroAfil " + '"' + ":" + '"' + " Nro Afiliado" + '"' + "," + '"' + "NOMBREAFIL" + '"' + ":" + '"' + " Nombre Afiliado" + '"' + "," + '"' + "Importe" + '"' + ":" + '"' + "Total" + '"' + "," + '"' + " UBIC" + '"' + ":" + '"' + "Ubicacion " + '"' + "," + '"' + " CODCONC" + '"' + ":" + '"' + " Concepto " + '"' + "," + '"' + "FePREST" + '"' + ":" + '"' + " Fecha Prestacion " + '"' + "," + '"' + "CANTFILAS" + '"' + ":" + '"' + "[Cantidad de registros]" + '"' + "},";

                        int Contador = dt.Rows.Count;
                        if (dt.Rows.Count > 0)
                        {
                            xSC = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                xSC = xSC + "{";
                                xSC = xSC + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["ID"].ToString() + '"' + ",";
                                xSC = xSC + '"' + "FILAFIL" + '"' + ":" + '"' + dt.Rows[i]["FILAFIL"].ToString() + '"' + ",";
                                xSC = xSC + '"' + "NROAFIL" + '"' + ":" + '"' + dt.Rows[i]["NROAFIL"].ToString() + '"' + ",";
                                xSC = xSC + '"' + "BENEFAFIL" + '"' + ":" + '"' + dt.Rows[i]["BENEFAFIL"].ToString() + '"' + ",";
                                xSC = xSC + '"' + "NOMBREAFIL" + '"' + ":" + '"' + dt.Rows[i]["NOMBREAFIL"].ToString() + '"' + ",";
                                xSC = xSC + '"' + "IMPORTE" + '"' + ":" + '"' + dt.Rows[i]["IMPORTE"].ToString() + '"' + ",";
                                xSC = xSC + '"' + "UBIC" + '"' + ":" + '"' + dt.Rows[i]["UBIC"].ToString() + '"' + ",";
                                xSC = xSC + '"' + "CODCONC" + '"' + ":" + '"' + dt.Rows[i]["CODCONC"].ToString() + '"' + ",";
                                xSC = xSC + '"' + "OCULTO" + '"' + ":" + '"' + dt.Rows[i]["OCULTO"].ToString() + '"' + ",";

                                xFEPREST = Convert.ToDateTime(dt.Rows[i]["FEPREST"]).ToString("dd/MM/yyyy");

                                xSC = xSC + '"' + "FEPREST" + '"' + ":" + '"' + xFEPREST + '"';
                                if (i == Contador - 1)
                                {
                                    xSC = xSC + "}";
                                }
                                else
                                {
                                    xSC = xSC + "},";
                                };
                            }
                            xSC = xSC + "]";

                            //objGSC.Add(new datosSinCab { dato = xSC });
                        }
                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "GRILLA SIN CABECERA:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            xSC = "[]";
                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        xSC = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            xSC = xSC + "{";
                            xSC = xSC + '"' + "ERROR" + '"' + " GRILLA SIN CABECERA:" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                xSC = xSC + "}";
                            }
                            else
                            {
                                xSC = xSC + "},";
                            };
                        }
                        xSC = xSC + "]";
                    }
                    con.Close();
                    return xSC;
                }
            }
        }

        public class StatusWs
        {
            public string Status { get; set; }
        }

        public bool GrabarHabilitacion(string xCUIT, string xInscRegNacPrest, string xbModalidad, string xCateg, string xHabilitacion, string xFecVenc, string xUsuario, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            //string xFilialParam = xSC;
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            bool AgregarHabNueva = false;
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();

            //    'Ubicar el maximo numero de informe
            double ID = 222;
            string gFilial = "23";
            ID = int.Parse(UltimoNroIdHab(gFilial, xModulo,  xBase,  xUserLog,  xFilialLog));

            AgregarHabNueva = false;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                string xFecha = string.Format("{0:d}", xFecVenc);
                sSql = "";
                sSql = sSql + "INSERT INTO GRAL_HABILITACIONES VALUES ('" + xCUIT + "',";
                sSql = sSql + "'" + xInscRegNacPrest + "','" + xbModalidad + "',";
                sSql = sSql + "'" + xCateg + "','" + xHabilitacion + "',";
                sSql = sSql + "'" + xFecha + "',";
                sSql = sSql + "'" + DateTime.Today.ToString("MM-d-yyyy") + "',";
                sSql = sSql + "'" + xUsuario + "'," + ID + ",'NO','NO')";

                try
                {
                    sqlTran = con.BeginTransaction();
                    ad.InsertCommand = new SqlCommand(sSql, con);
                    ad.InsertCommand.ExecuteNonQuery();
                    sqlTran.Commit();

                }
                catch (Exception ex)
                {
                    ID = 0;
                    // Attempt to roll back the transaction.
                    try
                    {
                        sqlTran.Rollback();
                    }
                    catch (Exception exRollback)
                    {
                        // Throws an InvalidOperationException if the connection 
                        // is closed or the transaction has already been rolled 
                        // back on the server.
                        ID = 0;
                    }
                }
                con.Close();
                return true;
            }

        }

        public string UltimoNroIdHab(string sFilial,string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string idHab;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "";
                sSQL = sSQL + "SELECT MAX(IDHABIL) AS MAXIMO FROM GRAL_HABILITACIONES WITH(NOLOCK) ";
                sSQL = sSQL + " WHERE  IDHABIL >=" + sFilial + "00000001 AND IDHABIL <=" + sFilial + "99999999";

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
                            idHab = (dt.Rows[0]["MAXIMO"].ToString()) + 1;
                        }
                        else
                        { idHab = "0"; }
                    }
                    catch (Exception ex)
                    {
                        idHab = "0";
                        con.Close();
                    }
                    return idHab;
                }
            }
        }

        public string ExisteIdHab(string xCUIT,string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string idHab;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "SELECT count(*)+1 as CANTIDAD ";
                sSQL = sSQL + " FROM GRAL_HABILITACIONES WITH(NOLOCK),CAB_documentacion_disca WITH(NOLOCK) ";
                sSQL = sSQL + " WHERE GRAL_HABILITACIONES.IDHABIL=CAB_documentacion_disca.IDPAPEL and GRAL_HABILITACIONES.CUIT='" + xCUIT + "'";

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
                            idHab = dt.Rows[0]["CANTIDAD"].ToString();
                        }
                        else
                        { idHab = "0"; }
                    }
                    catch (Exception ex)
                    {
                        idHab = "0";
                        con.Close();
                    }
                    return idHab;
                }
            }
        }

        public string ConsultarHabilitacion(string xCUIT,string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string sSql;
            string xHab = "[]";
            int xContaErr = 0;
            StatusWs objStatus = new StatusWs();

            if (xCUIT != null || xCUIT != "")
            { sSql = "SELECT * FROM GRAL_HABILITACIONES WITH(NOLOCK) WHERE CUIT ='" + xCUIT + "'"; }
            else
            { sSql = "SELECT * FROM GRAL_HABILITACIONES WITH(NOLOCK) ORDER BY CUIT"; }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
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
                            xHab = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                xHab = xHab + "{";
                                xHab = xHab + '"' + "CUIT" + '"' + ":" + '"' + dt.Rows[i]["CUIT"].ToString() + '"' + ",";
                                xHab = xHab + '"' + "INSCRNP" + '"' + ":" + '"' + dt.Rows[i]["INSCRNP"].ToString() + '"' + ",";
                                xHab = xHab + '"' + "MODALIDAD" + '"' + ":" + '"' + dt.Rows[i]["MODALIDAD"].ToString() + '"' + ",";
                                xHab = xHab + '"' + "CATEG" + '"' + ":" + '"' + dt.Rows[i]["CATEG"].ToString() + '"' + ",";
                                xHab = xHab + '"' + "NROHAB" + '"' + ":" + '"' + dt.Rows[i]["NROHAB"].ToString() + '"' + ",";
                                xHab = xHab + '"' + "FEVTO" + '"' + ":" + '"' + dt.Rows[i]["FEVTO"].ToString() + '"' + ",";
                                xHab = xHab + '"' + "FECARGA" + '"' + ":" + '"' + dt.Rows[i]["FECARGA"].ToString() + '"' + ",";
                                xHab = xHab + '"' + "USCRE" + '"' + ":" + '"' + dt.Rows[i]["USCRE"].ToString() + '"' + ",";
                                xHab = xHab + '"' + "IDHABIL" + '"' + ":" + '"' + dt.Rows[i]["IDHABIL"].ToString() + '"' + ",";
                                xHab = xHab + '"' + "IMPORTADO" + '"' + ":" + '"' + dt.Rows[i]["IMPORTADO"].ToString() + '"' + ",";
                                xHab = xHab + '"' + "MODIMPORT" + '"' + ":" + '"' + dt.Rows[i]["MODIMPORT"].ToString() + '"';
                                if (i < Contador - 1)
                                {
                                    xHab = xHab + "},";
                                }
                                else
                                {
                                    xHab = xHab + "}";
                                };
                            }
                            xHab = xHab + "]";

                        }
                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "ABM HABILITACIONES: NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            xHab = "[]";
                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        xHab = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            xHab = xHab + "{";
                            xHab = xHab + '"' + "ERROR" + '"' + " ABM HABILITACIONES:" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                xHab = xHab + "}";
                            }
                            else
                            {
                                xHab = xHab + "},";
                            };
                        }
                        xHab = xHab + "]";
                    }
                    con.Close();
                    return xHab;
                }
            }
        }

        public bool BorrarHabilitacion(string xCUIT,string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string sSql;
            string xHab = "";
            int xContaErr = 0;
            int ExisteEnDocumentacion = 0;
            bool xHabilborrada = false;
            SqlDataAdapter ad = new SqlDataAdapter();

            StatusWs objStatus = new StatusWs();

            ExisteEnDocumentacion = int.Parse(ExisteIdHab(xCUIT, xModulo,  xBase, xUserLog,  xFilialLog));


            if (ExisteEnDocumentacion == 0)
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
                {

                    sSql = "";
                    sSql = sSql + "DELETE FROM GRAL_HABILITACIONES WHERE CUIT='" + xCUIT + "'";

                    try
                    {

                        ad.InsertCommand = new SqlCommand(sSql, con);
                        ad.InsertCommand.ExecuteNonQuery();
                        xHabilborrada = true;
                    }
                    catch (Exception ex)
                    {
                        xHabilborrada = false;
                    }
                    con.Close();

                }
            }
            else
            {
                xHabilborrada = false;
            }

            return xHabilborrada;


        }


        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------

        public string FormatoFecha(string xDate, String xFormat, bool xTime)
        {
            string xDia = "";
            string xMes = "";
            string xAnio = "";
            int xLugares = 0;
            int i;
            bool xEsNumero;

            if (xDate == null || xDate == "" || xDate == "01/01/1900 0:00:00")
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
                case "dd-mm-yyyy":
                    xDate = xDia + "-" + xMes + "-" + xAnio;
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
        bool EsValNumerico(string xValor)
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

        public string ExportarExcelID(string idCabeceras, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            var respuestaJson = new StringBuilder();

            if (ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog) != "")
            {
                return "Todos los campos son bligatorios.";
            }

            if (idCabeceras == "")
            {
                return "Todos los campos son bligatorios.";
            }

            string sql = "SELECT HAYCOMENTARIOS,SUM(TOTAL) AS TOTAL, COUNT(TOTAL) AS CANTRENG, MAX(FEVTO) AS FEVTO, NOMBREAFIL,ID,FILAFIL,NROAFIL,BENEFAFIL,FEDESDE,FEHASTA, " +
                         "ESTADOADM,ESTADOCBLE,CODOS,CODCONC,UBICADM,FILDEBITO,NROEXPDTE,RENOS,OCULTO,DOCA_DISPONIBLE,ID_ENTE_GOB,FECRE,FECHAESTADOADM,FECHAESTADOCBLE " +
                         "FROM (SELECT CABECERAS.NOMBREAFIL,CABECERAS.ID,CABECERAS.FILAFIL,CABECERAS.NROAFIL,CABECERAS.BENEFAFIL,CABECERAS.FEDESDE, CABECERAS.FEHASTA, " +
                         "CABECERAS.ESTADOADM,CABECERAS.ESTADOCBLE,CODOS,GRAL_CONCEPTOS.PATOL_CODIF AS CODCONC,CABECERAS.UBICADM, " +
                         "DATEADD(d, -1, DATEADD(m, DATEDIFF(m, 0, DATEADD(m, 22, Max(FEPREST))) + 1, 0)) AS FEVTO, FILDEBITO,IMPORTE AS TOTAL,NROEXPDTE, " +
                         "CASE WHEN RNOS IS NULL THEN 'S/INFO' ELSE RNOS END AS RENOS, CABECERAS.CODCONC AS OCULTO,DOCA_DISPONIBLE,cab_comentarios.idcab AS HAYCOMENTARIOS, " +
                         "ID_ENTE_GOB,CABECERAS.FECRE, (SELECT MAX(FECRE) FROM CAB_SEGUIMIENTO CS WHERE CS.TIPO = 'ADM' AND CS.ESTADOANT <> CS.ESTADONUE AND CS.IDCAB = CABECERAS.ID) " +
                         "AS FECHAESTADOADM,(SELECT MAX(FECRE) FROM CAB_SEGUIMIENTO CS WHERE CS.TIPO = 'CBLE' AND CS.ESTADOANT <> CS.ESTADONUE AND CS.IDCAB = CABECERAS.ID) AS FECHAESTADOCBLE " +
                         "FROM (RENGLONES WITH(NOLOCK) RIGHT JOIN (CABECERAS WITH(NOLOCK) LEFT JOIN GRAL_AFILIADOS_SSS WITH(NOLOCK) ON CABECERAS.FILAFIL = GRAL_AFILIADOS_SSS.FILAFIL AND " + 
                         "CABECERAS.NROAFIL = GRAL_AFILIADOS_SSS.NROAFIL AND CABECERAS.BENEFAFIL = GRAL_AFILIADOS_SSS.BENEFAFIL AND CABECERAS.FEDESDE >= GRAL_AFILIADOS_SSS.FEDESDE AND " +
                         "CABECERAS.FEDESDE < CASE WHEN GRAL_AFILIADOS_SSS.FEHASTA IS NULL THEN '20991231' ELSE GRAL_AFILIADOS_SSS.FEHASTA END) ON CABECERAS.ID = RENGLONES.IDCAB) " +
                         "JOIN gral_conceptos WITH(NOLOCK) ON cabeceras.codconc=gral_conceptos.codconc LEFT JOIN cab_comentarios WITH(NOLOCK) ON cabeceras.id = cab_comentarios.idcab " +
                         "WHERE CABECERAS.ID>0  AND CABECERAS.ID IN (" + idCabeceras + ") " +
                         "GROUP BY CABECERAS.NOMBREAFIL, CABECERAS.ID, CABECERAS.FILAFIL, CABECERAS.NROAFIL, CABECERAS.BENEFAFIL,  CABECERAS.FEDESDE, CABECERAS.FEHASTA, CABECERAS.ESTADOADM, " +
                         "CABECERAS.ESTADOCBLE, CODOS, GRAL_CONCEPTOS.PATOL_CODIF, FILDEBITO, UBICADM,  NROEXPDTE, CASE WHEN RNOS IS NULL then 'S/INFO' else RNOS END,CABECERAS.CODCONC, " +
                         "CAB_COMENTARIOS.IDCAB,DOCA_DISPONIBLE,RENGLONES.IMPORTE , RENGLONES.ID, gral_conceptos.TIPO_VENCIMIENTO, ID_ENTE_GOB, CABECERAS.FECRE) AS TEMPORAL " +
                         "GROUP BY HAYCOMENTARIOS, NOMBREAFIL, ID, FILAFIL, NROAFIL, BENEFAFIL, FEDESDE, FEHASTA, UBICADM, ESTADOADM, ESTADOCBLE, CODOS, CODCONC, OCULTO, FILDEBITO, " +
                         "NROEXPDTE, RENOS,DOCA_DISPONIBLE, ID_ENTE_GOB,FECRE, FECHAESTADOADM, FECHAESTADOCBLE";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        respuestaJson.Append("[");


                        respuestaJson.Append("["+'"'+"Nro.Expdte;Apellido y Nombre;NROAFIL;Fecha desde;Fecha hasta;O.S.;Concepto;Vto;Cant.Fc;Importe;Estado.adm;Estado.cble;Empadronamiento;Id;Nro.Solicitud.Sur;Fecha.Creacion;Fecha.Estado.ADM;Fecha.Estado.CBLE"+'"'+"]");

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if(respuestaJson.Length > 1){
                                respuestaJson.Append(",[");
                            }else{
                                respuestaJson.Append("[");
                            }

                            respuestaJson.Append('"' + 
                                dt.Rows[i]["NROEXPDTE"].ToString() + ";" + 
                                dt.Rows[i]["NOMBREAFIL"].ToString()  + ";"+
                                dt.Rows[i]["FILAFIL"].ToString().PadLeft(2, '0') + "-" + dt.Rows[i]["NROAFIL"].ToString().PadLeft(7, '0') + "-" + dt.Rows[i]["BENEFAFIL"].ToString().PadLeft(2, '0') + ";"+
                                FormatoFecha(dt.Rows[i]["FEDESDE"].ToString(), "dd/mm/yyyy", false) + ";" + 
                                FormatoFecha(dt.Rows[i]["FEHASTA"].ToString(), "dd/mm/yyyy", false) + ";"+ 
                                dt.Rows[i]["CODOS"].ToString() + ";" +
                                dt.Rows[i]["CODCONC"].ToString() + ";"+ 
                                FormatoFecha(dt.Rows[i]["FEVTO"].ToString(), "dd/mm/yyyy", false) + ";"+
                                dt.Rows[i]["CANTRENG"].ToString() + ";"+
                                dt.Rows[i]["TOTAL"].ToString() + ";"+
                                dt.Rows[i]["ESTADOADM"].ToString() + ";"+
                                dt.Rows[i]["ESTADOCBLE"].ToString() + ";"+
                                dt.Rows[i]["RENOS"].ToString() +  ";"+
                                dt.Rows[i]["ID"].ToString() + ";"+
                                dt.Rows[i]["ID_ENTE_GOB"].ToString() + ";"+ 
                                FormatoFecha(dt.Rows[i]["FECRE"].ToString(), "dd/mm/yyyy", false) +";" + 
                                FormatoFecha(dt.Rows[i]["FECHAESTADOADM"].ToString(), "dd/mm/yyyy", false) +  ";" + 
                                FormatoFecha(dt.Rows[i]["FECHAESTADOCBLE"].ToString(), "dd/mm/yyyy", false) + '"');

                            respuestaJson.Append("]");
                        }
                        respuestaJson.Append("]");
                    }
                }
            }

            return respuestaJson.ToString();
        }  
    }
}