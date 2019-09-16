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
using WcfServiceAPE;
using System.IO;
using System.Transactions;

namespace WcfServiceAPE
{
    [ServiceBehavior(Namespace = "http://ar.com.osde/osgapeservice/", Name = "OSGAPEBackendService")]
    public class WS2 : MenuABM
    {
        string MenuABM.modifDiscapacidad(string xCUIT, string xApellido, string xNombre, string xTelefono1, string xEmail, string xCodEsp, string xTipo, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();

            string sSql = "";
            string xMensaje = "";

            

            SqlDataAdapter ad = new SqlDataAdapter();

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            string xNombrePrestador = xApellido.Trim() + " " + xNombre.Trim();
            string ID = "";

            if (ValidarCuit(xCUIT) == false
                || xNombre == null || xNombre == "" || xNombre.Length > 45 
                || xApellido == null || xApellido == "" || xApellido.Length > 45 
                || xCodEsp == null || xCodEsp == "" || validarEspecialidad(xCodEsp, xModulo, xBase, xUserLog, xFilialLog) == false
                || xTipo == null || xTipo == "" || (xTipo != "P" && xTipo != "I" && xTipo != "T")
               )
            {
                return "Su petición no puede realizarse.";
               
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "";
                sSql = sSql + "UPDATE GRAL_PRESTADORES ";
                sSql = sSql + "SET NOMPRESTADOR='" + xNombrePrestador + "',";
                sSql = sSql + "NOMBRE='" + xNombre + "',";
                sSql = sSql + "APELLIDO='" + xApellido + "',";
                sSql = sSql + "TEL1='" + xTelefono1 + "',";
                sSql = sSql + "TEL2='" + xEmail + "',";
                sSql = sSql + "TIPO='" + xTipo + "',";
                sSql = sSql + "CODESP='" + xCodEsp + "'";
                sSql = sSql + " WHERE CUIT='" + xCUIT + "'";

                try
                {
                    con.Open();

                    ad.InsertCommand = new SqlCommand(sSql, con);
                    ad.InsertCommand.ExecuteNonQuery();

                    ID = "";
                }
                catch (Exception ex)
                {
                    con.Close();
                    return "Su petición no puede realizarse.";
                }
                con.Close();
                return ID;
            }
        }

        string MenuABM.altaDiscapacidad(string xCUIT, string xApellido, string xNombre, string xTelefono1, string xEmail, string xCodEsp, string xTipo, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";           
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return "Su petici&oacute;n no puede realizarse.";
            }

            string xNombrePrestador = xApellido.Trim() + " " + xNombre.Trim();
            string ID = xCUIT;

           
            WS2 objFuncionesGrales = new WS2();
            bool bMotivos = objFuncionesGrales.consultaCUIT(xCUIT, xModulo,  xBase,  xUserLog,  xFilialLog);
            if (bMotivos == true)
            {
                xMensaje = "El prestador ya existe.";
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = sSql + "INSERT INTO GRAL_PRESTADORES VALUES ('" + ID + "',";
                sSql = sSql + "'" + xNombrePrestador + "','" + xCUIT + "',";
                sSql = sSql + "'" + xNombre + "','" + xApellido + "',";
                sSql = sSql + "'" + xTelefono1 + "',";
                sSql = sSql + "'" + xEmail + "',";
                sSql = sSql + "'" + xTipo + "',";
                sSql = sSql + "'" + xCodEsp + "','NO','NO')";

                con.Open();
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("PrestadoresTransaction");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;
                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    ID = "";
                }
                catch (Exception e)
                {
                    con.Close();
                    return "No se pudo realizar el Alta.";
                }

                con.Close();
                return ID;
            }
        }

        string MenuABM.consultaDiscapacidad(string xCUIT, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string ID = xCUIT;
            string jPrest = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                if (xCUIT != null && xCUIT != "")
                {
                    sSql = "SELECT * FROM GRAL_PRESTADORES WITH(NOLOCK) WHERE CODPRESTADOR ='" + xCUIT + "'";
                }
                else
                {
                    sSql = "SELECT TOP 10 * FROM GRAL_PRESTADORES WITH(NOLOCK) ";
                }

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
                            jPrest = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                jPrest = jPrest + "{";
                                jPrest = jPrest + '"' + "Cuit" + '"' + ":" + '"' + dt.Rows[i]["CUIT"].ToString() + '"' + ",";
                                jPrest = jPrest + '"' + "NOMBRE" + '"' + ":" + '"' + dt.Rows[i]["NOMBRE"].ToString() + '"' + ",";
                                jPrest = jPrest + '"' + "APELLIDO" + '"' + ":" + '"' + dt.Rows[i]["APELLIDO"].ToString() + '"' + ",";
                                jPrest = jPrest + '"' + "Especialidad" + '"' + ":" + '"' + dt.Rows[i]["CODESP"].ToString() + '"' + ",";
                                jPrest = jPrest + '"' + "Tipo" + '"' + ":" + '"' + dt.Rows[i]["TIPO"].ToString() + '"' + ",";
                                jPrest = jPrest + '"' + "Tel" + '"' + ":" + '"' + dt.Rows[i]["TEL1"].ToString() + '"' + ",";
                                jPrest = jPrest + '"' + "mail" + '"' + ":" + '"' + dt.Rows[i]["TEL2"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    jPrest = jPrest + "},";
                                }
                                else
                                {
                                    jPrest = jPrest + "}";
                                };
                            }
                            jPrest = jPrest + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            jPrest = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jPrest = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jPrest = jPrest + "{";
                            jPrest = jPrest + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jPrest = jPrest + "}";
                            }
                            else
                            {
                                jPrest = jPrest + "},";
                            };
                        }
                        jPrest = jPrest + "]";
                    }
                    con.Close();
                    return jPrest;

                }
            }

        }
        //-----------------------------------------------------------------------------------
        string MenuABM.datosDiscapacidad(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string jPrest = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT * FROM GRAL_ESPECIALIDADDISCA WITH(NOLOCK) WHERE ESTESP ='A'";

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
                            jPrest = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                jPrest = jPrest + "{";
                                jPrest = jPrest + '"' + "Especialidaddisca" + '"' + ":" + '"' + dt.Rows[i]["CODESP"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    jPrest = jPrest + "},";
                                }
                                else
                                {
                                    jPrest = jPrest + "}";
                                };
                            }
                            jPrest = jPrest + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            jPrest = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jPrest = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jPrest = jPrest + "{";
                            jPrest = jPrest + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jPrest = jPrest + "}";
                            }
                            else
                            {
                                jPrest = jPrest + "},";
                            };
                        }
                        jPrest = jPrest + "]";
                    }
                    con.Close();
                    return jPrest;

                }
            }

        }
        //-----------------------------------------------------------------------------------
        // ABM Afiliados
        string MenuABM.buscar_afiliados(string xNroAfiliado, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string bAfil = "";
            string xFdesde = "";
            string xFnac = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                if (xNroAfiliado != null && xNroAfiliado != "")
                {
                    sSql = "SELECT * FROM GRAL_AFILIADOS WITH(NOLOCK) ";
                    sSql = sSql + "WHERE nroafil like '" + xNroAfiliado + "%'";
                }
                else
                {
                    sSql = "SELECT TOP 10 * FROM GRAL_AFILIADOS WITH(NOLOCK) ";
                }

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
                            bAfil = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                bAfil = bAfil + "{";
                                bAfil = bAfil + '"' + "Filafil" + '"' + ":" + '"' + dt.Rows[i]["Filafil"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "nroafil" + '"' + ":" + '"' + dt.Rows[i]["nroafil"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "beneafil" + '"' + ":" + '"' + dt.Rows[i]["benefafil"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "nombre" + '"' + ":" + '"' + dt.Rows[i]["nombre"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "apellido" + '"' + ":" + '"' + dt.Rows[i]["apellido"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "sexo" + '"' + ":" + '"' + dt.Rows[i]["sexo"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "dni" + '"' + ":" + '"' + dt.Rows[i]["dni"].ToString() + '"' + ",";

                                xFdesde = dt.Rows[i]["fechanac"].ToString();
                                xFnac = FormatoFecha(xFdesde, "dd/mm/yyyy", false);

                                bAfil = bAfil + '"' + "fenac" + '"' + ":" + '"' + xFnac + '"' + ",";

                                bAfil = bAfil + '"' + "codhiv" + '"' + ":" + '"' + dt.Rows[i]["codhiv"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "fildeb" + '"' + ":" + '"' + dt.Rows[i]["fildebito"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "telefonoper" + '"' + ":" + '"' + dt.Rows[i]["tel1"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "telefonoalt" + '"' + ":" + '"' + dt.Rows[i]["tel2"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "cap" + '"' + ":" + '"' + dt.Rows[i]["cap"].ToString() + '"' + ",";
                                bAfil = bAfil + '"' + "pal" + '"' + ":" + '"' + dt.Rows[i]["plan_afil"].ToString() + '"';


                                if (i < Contador - 1)
                                {
                                    bAfil = bAfil + "},";
                                }
                                else
                                {
                                    bAfil = bAfil + "}";
                                };
                            }
                            bAfil = bAfil + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "AFILIADOS:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            bAfil = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        bAfil = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            bAfil = bAfil + "{";
                            bAfil = bAfil + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                bAfil = bAfil + "}";
                            }
                            else
                            {
                                bAfil = bAfil + "},";
                            };
                        }
                        bAfil = bAfil + "]";
                    }
                    con.Close();
                    return bAfil;

                }
            }
        }
        string MenuABM.cargaCAP(string xfilDebito, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string cargaCAP = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT * FROM GRAL_CAPS WITH(NOLOCK) ";
                sSql = sSql + " WHERE FILIAL='" + xfilDebito + "' AND MODIMPORT='A'";
                sSql = sSql + " ORDER BY CODCAP";

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
                            cargaCAP = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                cargaCAP = cargaCAP + "{";
                                cargaCAP = cargaCAP + '"' + "cap" + '"' + ":" + '"' + dt.Rows[i]["CODCAP"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    cargaCAP = cargaCAP + "},";
                                }
                                else
                                {
                                    cargaCAP = cargaCAP + "}";
                                };
                            }
                            cargaCAP = cargaCAP + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "CARGACAP:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cargaCAP = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cargaCAP = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cargaCAP = cargaCAP + "{";
                            cargaCAP = cargaCAP + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cargaCAP = cargaCAP + "}";
                            }
                            else
                            {
                                cargaCAP = cargaCAP + "},";
                            };
                        }
                        cargaCAP = cargaCAP + "]";
                    }
                    con.Close();
                    return cargaCAP;

                }
            }

        }
        string MenuABM.cargaPlan(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string cargaCAP = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT Descripcion_plan FROM GRAL_PLANES ORDER BY Descripcion_plan ";

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
                            cargaCAP = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                cargaCAP = cargaCAP + "{";
                                cargaCAP = cargaCAP + '"' + "plan" + '"' + ":" + '"' + dt.Rows[i]["Descripcion_plan"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    cargaCAP = cargaCAP + "},";
                                }
                                else
                                {
                                    cargaCAP = cargaCAP + "}";
                                };
                            }
                            cargaCAP = cargaCAP + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "CARGAPLAN:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cargaCAP = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cargaCAP = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cargaCAP = cargaCAP + "{";
                            cargaCAP = cargaCAP + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cargaCAP = cargaCAP + "}";
                            }
                            else
                            {
                                cargaCAP = cargaCAP + "},";
                            };
                        }
                        cargaCAP = cargaCAP + "]";
                    }
                    con.Close();
                    return cargaCAP;

                }
            }

        }
        string MenuABM.alta_afiliado(string xFilial, string xnroAfil, string xBeneAfil, string xNombre, string xApellido, string xSexo, string xDni, string xFechnac, string xCodhiv, string xFildeb, string xTelefonoper, string xTelefonoalt, string xCap, string xPlan, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            string sSql = "";
            string xMensaje = "";
            string AgregarHabNueva = "";

            StatusWs objStatus = new StatusWs();
            SqlDataAdapter ad = new SqlDataAdapter();

            try
            {
                xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);

                if (xMensaje != "")
                {
                    return xMensaje;
                }

                if (xFechnac.ToString() == null || xFechnac.ToString() == "" || xFechnac.ToString() == "NULL")
                {
                    xFechnac = "";
                }
                else
                {
                    xFechnac = FormatoFecha(xFechnac, "yyyy/mm/dd", true);
                }
            }
            catch (Exception e)
            {
                return "Error al procesar los datos:" + e.Message;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
         
                sSql = "INSERT INTO GRAL_AFILIADOS VALUES ('" + xFilial + "',";
                sSql = sSql + "'" + xnroAfil + "','" + xBeneAfil + "',";
                sSql = sSql + "'" + xNombre + "','" + xApellido + "',";
                sSql = sSql + "'" + xSexo + "',";

                if (xDni == "" || xDni == null || xDni == "NULL" || xDni == "null"){
                    sSql = sSql + "" + "NULL" + ",";
                }else{
                    sSql = sSql + "'" + xDni + "',";
                }

                if (xFechnac == "") { 
                    sSql = sSql + "NULL ,";
                }
                else
                {
                    sSql = sSql + "'" + xFechnac + "',";
                }
                sSql = sSql + "'" + xCodhiv + "',";
                sSql = sSql + "'" + xFildeb + "',";
                sSql = sSql + "'" + xTelefonoper + "',";
                sSql = sSql + "'" + xTelefonoalt + "',";
                sSql = sSql + "'" + "NO','NO',";
                sSql = sSql + "'" + xCap + "',";
                sSql = sSql + "'" + xPlan + "')";

                con.Open();
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("AltaAfiliadosTransaction");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;
                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    AgregarHabNueva = "";
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message ;
                }

                con.Close();
                return AgregarHabNueva;
            }
        }

        string MenuABM.mod_afiliado(string xFilial, string xnroAfil, string xBeneAfil, string xNombre, string xApellido, string xSexo, string xDni, string xFechnac, string xCodhiv, string xFildeb, string xTelefonoper, string xTelefonoalt, string xCap, string xPlan, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            SqlTransaction transaction;

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return "Error no se pudo realizar la Modificacion del Afiliado.";
            }

            if (xFechnac == null || xFechnac == "" || xFechnac == "NULL")
            {
                xFechnac = "";
            }
            else
            {
                xFechnac = FormatoFecha(xFechnac, "yyyy/mm/dd", true);
            }
           

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "UPDATE GRAL_AFILIADOS ";
                sSql = sSql + "SET nombre='" + xNombre + "', ";
                sSql = sSql + "apellido='" + xApellido + "', ";
                sSql = sSql + "sexo ='" + xSexo + "', ";

                if (xDni == "" || xDni == null || xDni == "NULL" || xDni == "null")
                {
                    sSql = sSql + "dni=" + "NULL" + ", ";
                }
                else
                {
                    sSql = sSql + "dni='" + xDni + "', ";
                }

                if (xFechnac == "")
                {
                    sSql = sSql + "fechanac=" + "NULL" + ", ";
                }
                else
                {
                    sSql = sSql + "fechanac='" + xFechnac + "', ";
                }


                sSql = sSql + "codhiv='" + xCodhiv + "', ";
                sSql = sSql + "fildebito='" + xFildeb + "', ";
                sSql = sSql + "tel1='" + xTelefonoper + "', ";
                sSql = sSql + "tel2='" + xTelefonoalt + "', ";
                sSql = sSql + "modimport=CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END, ";
                sSql = sSql + "cap='" + xCap + "', ";
                sSql = sSql + "plan_afil='" + xPlan + "' ";
                sSql = sSql + "WHERE filafil='" + xFilial + "' and nroafil='" + xnroAfil + "' and benefafil='" + xBeneAfil + "'";

                con.Open();
                SqlCommand command = con.CreateCommand();

                transaction = con.BeginTransaction("ModifAfiliadosTransaction");
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
                    transaction.Rollback("ModifAfiliadosTransaction");
                    return "Error no se pudo realizar la Modificacion del Afiliado.";
                }

                con.Close();
                return "";
            }
        }

        string MenuABM.eliminar_rnos(string xfilAfil, string xnroAfil, string xBeneAfil, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            //string xFilialParam = xSC;
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            string ModificarAfiliado = "";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = sSql + "DELETE FROM GRAL_AFILIADOS_SSS ";
                sSql = sSql + " WHERE filafil='" + xfilAfil + "'";
                sSql = sSql + " AND nroafil='" + xnroAfil + "' AND benefafil='" + xBeneAfil + "'";

                try
                {
                    con.Open();
                    //sqlTran = con.BeginTransaction();
                    ad.DeleteCommand = new SqlCommand(sSql, con);
                    ad.DeleteCommand.ExecuteNonQuery();
                    //sqlTran.Commit();
                    ModificarAfiliado = "El historial ha sido purgado.";

                }
                catch (Exception ex)
                {
                    ModificarAfiliado = "Error al purgar historial.";
                }
                con.Close();
                return ModificarAfiliado;
            }
        }
        //-----------------------------------------------------------------------------------
        string MenuABM.conceptosactivos(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string cptosact = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT * FROM GRAL_CONCEPTOS  WITH(NOLOCK)";
                sSql = sSql + " WHERE ESTCONC = 'A'";
                sSql = sSql + " ORDER BY CODCONC";


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
                            cptosact = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                cptosact = cptosact + "{";
                                cptosact = cptosact + '"' + "general" + '"' + ":" + '"' + dt.Rows[i]["CODCONC"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    cptosact = cptosact + "},";
                                }
                                else
                                {
                                    cptosact = cptosact + "},";
                                    cptosact = cptosact + "{";
                                    cptosact = cptosact + '"' + "general" + '"' + ":" + '"' + "GENERAL" + '"';
                                    cptosact = cptosact + "}";
                                };
                            }
                            cptosact = cptosact + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "cptosact:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cptosact = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cptosact = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cptosact = cptosact + "{";
                            cptosact = cptosact + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cptosact = cptosact + "}";
                            }
                            else
                            {
                                cptosact = cptosact + "},";
                            };
                        }
                        cptosact = cptosact + "]";
                    }
                    con.Close();
                    return cptosact;

                }
            }
        }

        string MenuABM.tipodocumento(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string tipodoc = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT DISTINCT TIPODOC FROM GRAL_DOCUMENTACION  WITH(NOLOCK)";
                sSql = sSql + " ORDER BY TIPODOC";


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
                            tipodoc = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                tipodoc = tipodoc + "{";
                                tipodoc = tipodoc + '"' + "tipodoc" + '"' + ":" + '"' + dt.Rows[i]["TIPODOC"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    tipodoc = tipodoc + "},";
                                }
                                else
                                {
                                    tipodoc = tipodoc + "}";
                                };
                            }
                            tipodoc = tipodoc + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "tipodoc:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            tipodoc = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        tipodoc = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            tipodoc = tipodoc + "{";
                            tipodoc = tipodoc + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                tipodoc = tipodoc + "}";
                            }
                            else
                            {
                                tipodoc = tipodoc + "},";
                            };
                        }
                        tipodoc = tipodoc + "]";
                    }
                    con.Close();
                    return tipodoc;

                }
            }
        }
        string MenuABM.obtenerdocument(string xnrodoc, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            string nrodoc = "";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                if (xnrodoc != null && xnrodoc != "")
                {

                    sSql = "SELECT CODDOC AS Codigo,RESP_DOC_DEFAULT AS RespDoc, DESCDOC AS Descripcion, CODCONC AS Concepto, ESTDOC AS Estado, TIPODOC ";
                    sSql = sSql + " FROM GRAL_DOCUMENTACION  WITH(NOLOCK)";
                    sSql = sSql + "	Where CODDOC = '" + xnrodoc + "'";
                    sSql = sSql + " ORDER BY CODDOC";

                }
                else
                {
                    sSql = "SELECT CODDOC AS Codigo,RESP_DOC_DEFAULT AS RespDoc, DESCDOC AS Descripcion, CODCONC AS Concepto, ESTDOC AS Estado, TIPODOC ";
                    sSql = sSql + " FROM GRAL_DOCUMENTACION  WITH(NOLOCK)";
                    sSql = sSql + " ORDER BY CODDOC";
                }
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
                            nrodoc = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                nrodoc = nrodoc + "{";
                                nrodoc = nrodoc + '"' + "Codigo" + '"' + ":" + '"' + dt.Rows[i]["Codigo"].ToString() + '"' + ",";
                                nrodoc = nrodoc + '"' + "RespDoc" + '"' + ":" + '"' + dt.Rows[i]["RespDoc"].ToString() + '"' + ",";
                                nrodoc = nrodoc + '"' + "Descripcion" + '"' + ":" + '"' + dt.Rows[i]["Descripcion"].ToString() + '"' + ",";
                                nrodoc = nrodoc + '"' + "Concepto" + '"' + ":" + '"' + dt.Rows[i]["Concepto"].ToString() + '"' + ",";
                                nrodoc = nrodoc + '"' + "Estado" + '"' + ":" + '"' + dt.Rows[i]["Estado"].ToString() + '"' + ",";
                                nrodoc = nrodoc + '"' + "Tipodoc" + '"' + ":" + '"' + dt.Rows[i]["TIPODOC"].ToString() + '"';


                                if (i < Contador - 1)
                                {
                                    nrodoc = nrodoc + "},";
                                }
                                else
                                {
                                    nrodoc = nrodoc + "}";
                                };
                            }
                            nrodoc = nrodoc + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            nrodoc = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        nrodoc = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            nrodoc = nrodoc + "{";
                            nrodoc = nrodoc + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                nrodoc = nrodoc + "}";
                            }
                            else
                            {
                                nrodoc = nrodoc + "},";
                            };
                        }
                        nrodoc = nrodoc + "]";
                    }
                    con.Close();
                    return nrodoc;

                }
            }

        }
        public bool consultaCUIT(string xCUIT, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            bool xExisteCUIT = false;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "SELECT * FROM GRAL_PRESTADORES WITH(NOLOCK) WHERE CUIT ='" + xCUIT + "'";


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
                            xExisteCUIT = true;
                        }
                        else
                        { xExisteCUIT = false; }
                    }
                    catch (Exception ex)
                    {
                        xExisteCUIT = false; ;
                        con.Close();
                    }
                    return xExisteCUIT;
                }
            }
        }

        public bool consultaPRESTADOR(string xCodPres, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            bool xExistePREST = false;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "SELECT * FROM GRAL_PRESTADORES WITH(NOLOCK) WHERE CODPRESTADOR ='" + xCodPres + "'";


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
                            xExistePREST = true;
                        }
                        else
                        { xExistePREST = false; }
                    }
                    catch (Exception ex)
                    {
                        xExistePREST = false; ;
                        con.Close();
                    }
                    return xExistePREST;
                }
            }
        }
        public bool VerificaExiteDoc(string xDoc, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            bool xExisteDoc = false;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "SELECT CODDOC FROM GRAL_DOCUMENTACION  WITH(NOLOCK)";
                sSQL = sSQL + " Where CODDOC = '" + xDoc + "'";

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
                            xExisteDoc = true;
                        }
                        else
                        { xExisteDoc = false; }
                    }
                    catch (Exception ex)
                    {
                        xExisteDoc = false; ;
                        con.Close();
                    }
                    return xExisteDoc;
                }
            }
        }

        public Boolean longitudCincuenta(string campo)
        {
            if (campo.Length > 50)
            {
                return false;
            }

            return true;
        }

        public Boolean longitudUno(string campo)
        {
            if (campo.Length > 1)
            {
                return false;
            }

            return true;
        }

        string MenuABM.altadocasociado(string xcodigo, string xconcepto, string xdescripcion, string xtipodoc, string xrespdoc, string xestado, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            try
            {
                if(! (longitudCincuenta(xcodigo) && longitudCincuenta(xconcepto) && longitudCincuenta(xdescripcion) && longitudCincuenta(xtipodoc) && longitudCincuenta(xrespdoc) && longitudUno(xestado)))
                {
                    return "Error: los campos ingresados superan el tamaño establecido.";
                }
            }
            catch (Exception e)
            {
                return "ERROR: El documento NO fue dado de alta con éxito.";
            }

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string AgregarDocNuevo = "";
            string v_respdoc = "";
            bool xExisteDoc = false;
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);

            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                if (xrespdoc != "N")
                {
                    v_respdoc = xrespdoc;
                }
                else
                {
                    v_respdoc = "P";
                }
                xExisteDoc = VerificaExiteDoc(xcodigo, xModulo, xBase, xUserLog, xFilialLog);
                if (xExisteDoc == false)
                {
                    sSql = "";
                    sSql = sSql + "INSERT INTO GRAL_DOCUMENTACION VALUES ('" + xcodigo + "',";
                    sSql = sSql + "'" + xdescripcion + "','" + xconcepto + "',";
                    sSql = sSql + "'" + xtipodoc + "','" + xestado + "','NO','NO',";
                    sSql = sSql + "'" + xrespdoc + "')";

                    con.Open();

                    ad.InsertCommand = new SqlCommand(sSql, con);

                    if (ad.InsertCommand.ExecuteNonQuery() > 0) // Logro el insert
                    {
                        sSql = " INSERT INTO CAB_DOCUMENTACION ";
                        sSql = sSql + "SELECT CABECERAS.ID, '" + xcodigo + "', 'N', 'AUTO', rtrim(convert(char(23),getdate(),111)), rtrim(convert(char(23),getdate(),108)), NULL, NULL, NULL, 'ALTA DOC', NULL, '" + v_respdoc + "'";
                        sSql = sSql + " FROM CABECERAS  WITH(NOLOCK)";
                        sSql = sSql + " WHERE CABECERAS.CODCONC='" + xconcepto + "' AND CABECERAS.ESTADOCBLE NOT IN ('ANALIZADO','DENEGADO','PRESENTADO', 'NOTIFICADO','COBRADO','ARCHIVADO','NO APE')";

                    }
                    try
                    {

                        ad.InsertCommand = new SqlCommand(sSql, con);
                        ad.InsertCommand.ExecuteNonQuery();
                        AgregarDocNuevo =  "";

                    }
                    catch (Exception ex)
                    {
                        AgregarDocNuevo = "ERROR: El documento NO fue dado de alta con éxito.";
                    }
                }
                else
                {
                    AgregarDocNuevo = "ERROR: El código ya existe, no se puede dar de alta";
                    con.Close();

                }
                return AgregarDocNuevo;
            }
        }
        string MenuABM.modifdocumento(string xcodigo, string xconcepto, string xdescripcion, string xtipodoc, string xestado, string xrespdoc, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xRESP_DOC_DEFAULT = "";
            string cptomodifmsg = "";
            string Ret_cptomodifmsg = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT RESP_DOC_DEFAULT FROM GRAL_DOCUMENTACION  WITH(NOLOCK) WHERE CODDOC = '" + xcodigo + "' AND CODCONC = '" + xconcepto + "'";
                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();

                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(dt);

                    int Contador = dt.Rows.Count;
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            xRESP_DOC_DEFAULT = dt.Rows[i]["RESP_DOC_DEFAULT"].ToString();
                        }
                    }

                    sSql = "";
                    sSql = sSql + "UPDATE GRAL_DOCUMENTACION ";
                    sSql = sSql + "SET DESCDOC='" + xdescripcion + "',";
                    sSql = sSql + "TIPODOC='" + xtipodoc + "',";
                    sSql = sSql + "ESTDOC='" + xestado + "',";
                    sSql = sSql + "IMPORTADO='" + "MODIF" + "',";
                    sSql = sSql + "MODIMPORT='" + "SI" + "',";
                    sSql = sSql + "RESP_DOC_DEFAULT='" + xrespdoc + "'";
                    sSql = sSql + " WHERE CODDOC='" + xcodigo + "'";
                    sSql = sSql + " AND CODCONC='" + xconcepto + "'";

                    try
                    {
                        ad.InsertCommand = new SqlCommand(sSql, con);
                        ad.InsertCommand.ExecuteNonQuery();
                        Ret_cptomodifmsg = "[{" + '"' + "ModifDocumentacion" + '"' + ":" + '"' + "1" + '"' + "}]";
                    } catch (Exception ex) {
                        Ret_cptomodifmsg = "[{" + '"' + "ModifDocumentacion" + '"' + ":" + '"' + "0" + '"' + "}]";

                    }
                    if (xRESP_DOC_DEFAULT == xrespdoc)
                    {
                        cptomodifmsg = "El documento fue modificado.";
                    } else {
                        cptomodifmsg = "El doc fue modificado, el cbio en el resp de la documentación entrará en vigencia con los nuevos expedientes.";
                    }
                    con.Close();
                    return cptomodifmsg;
                }
            }
        }
        //-----------------------------------------------------------------------------------
        //ABM MOTIVOS
        string ValidarTipoMotivo(string xtipomotivo)
        {
            string xMensaje = "Su petición no puede realizarse.";
            switch (xtipomotivo)
            {
                case "ANULACION":
                    return xMensaje = "";

                case "AJUSTE INGRESO":
                    return xMensaje = "";

                case "CERRADO":
                    return xMensaje = "";

                case "SIN EMPADRONAR":
                    return xMensaje = "";

                default:
                    return xMensaje;

            }

        }
        string MenuABM.buscar_motivos(string xtipomotivo, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xTablaMotivos = "";
            string bMotivos = "";

            xMensaje = ValidarTipoMotivo(xtipomotivo);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            switch (xtipomotivo)
            {
                case "ANULACION":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSANULACION";
                    break;
                case "AJUSTE INGRESO":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSAJUSTEINGRESO";
                    break;
                case "CERRADO":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSSINGESTION";
                    break;
                case "SIN EMPADRONAR":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSSINEMPADRONAR";
                    break;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "Select * from " + xTablaMotivos + " WITH(NOLOCK) Order By CODANUL Desc";

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
                            bMotivos = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                bMotivos = bMotivos + "{";
                                bMotivos = bMotivos + '"' + "CodAnul" + '"' + ":" + '"' + dt.Rows[i]["CODANUL"].ToString() + '"' + ",";
                                bMotivos = bMotivos + '"' + "DescLarga" + '"' + ":" + '"' + dt.Rows[i]["DESCANUL"].ToString() + '"' + ",";
                                bMotivos = bMotivos + '"' + "DescCorta" + '"' + ":" + '"' + dt.Rows[i]["DESC2ANUL"].ToString() + '"' + ",";
                                bMotivos = bMotivos + '"' + "Estado" + '"' + ":" + '"' + dt.Rows[i]["ESTADO"].ToString() + '"' + ",";
                                bMotivos = bMotivos + '"' + "tipomotivo" + '"' + ":" + '"' + xtipomotivo + '"';

                                if (i < Contador - 1)
                                {
                                    bMotivos = bMotivos + "},";
                                }
                                else
                                {
                                    bMotivos = bMotivos + "}";
                                };
                            }
                            bMotivos = bMotivos + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "MOTIVOS: NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            bMotivos = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        bMotivos = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            bMotivos = bMotivos + "{";
                            bMotivos = bMotivos + '"' + "ERROR EN MOTIVOS:" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                bMotivos = bMotivos + "}";
                            }
                            else
                            {
                                bMotivos = bMotivos + "},";
                            };
                        }
                        bMotivos = bMotivos + "]";
                    }
                    con.Close();
                    return bMotivos;

                }
            }

        }
        string MenuABM.altamotivo(string xtipomotivo, string xdesclarg, string xdescorta, string xestado, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMaxNro = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xTablaMotivos = "";
            string bMotivos = "";
            string xReturn = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            xMensaje = ValidarTipoMotivo(xtipomotivo);
            if (xMensaje != ""
                || EsStrNuloBlanco(xdesclarg) == true || EsStrNuloBlanco(xdescorta) == true || EsStrNuloBlanco(xestado) == true || xdesclarg.Length > 50 || xdescorta.Length > 50
                || (xestado != "A" && xestado != "I"))
            {
                return xMensaje = "Su petición no puede realizarse.";
            }


            switch (xtipomotivo)
            {
                case "ANULACION":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSANULACION";
                    break;
                case "AJUSTE INGRESO":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSAJUSTEINGRESO";
                    break;
                case "CERRADO":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSSINGESTION";
                    break;
                case "SIN EMPADRONAR":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSSINEMPADRONAR";
                    break;
            }
            xMaxNro = UltimoNroId(xTablaMotivos, "CODANUL",  xModulo,  xBase,  xUserLog,  xFilialLog);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "INSERT INTO " + xTablaMotivos + " VALUES (";
                sSql = sSql + "'" + xMaxNro + "','" + xdesclarg + "',";
                sSql = sSql + "'" + xdescorta + "'," + "NULL,NULL,";
                sSql = sSql + "'" + xestado + "')";

                //Comienzo la transacción...

                con.Open();
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("AltaMotivosTransaction");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;
                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    xReturn = "1";
                }
                catch (Exception ex)
                {
                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("AltaMotivosTransaction");
                    xReturn = "0";
                }
                con.Close();
                // Regenero la Lista con el nuevo registro...
                //declaro un Objeto refereciando al metodo que necesito para que no de error:
                //"Error 43 - Se requiere una referencia de objeto para el campo, método o propiedad no estáticos"
                WS2 objFuncionesGrales = new WS2();
                bMotivos = objFuncionesGrales.buscar_motivos_lista(xtipomotivo, xModulo, xBase, xUserLog, xFilialLog);
                return bMotivos;
            }
        }
        public string FormatoFac(string xNroFAC)
        {
            //Valor de entrada NroFAC=A23456789012; Valor a insertar NroFac= 'A 2345-67890123'
            if (xNroFAC.Length >= 6) { 
                xNroFAC = xNroFAC.Substring(0, 1) + " " + xNroFAC.Substring(1, 4) + "-" + xNroFAC.Substring(5);
            }
            else if (xNroFAC.Length < 6 && xNroFAC.Length >= 2){
                xNroFAC = xNroFAC.Substring(0, 1) + " " + xNroFAC.Substring(1, 4) ;
            }
            
            return xNroFAC;
        }
        public string FormatoRemito(string xNroRem)
        {
            //Valor recibido NroRemito= 000200292336 valor a insertar 0002-00292336
            if (xNroRem.Length >= 5) { 
                xNroRem = xNroRem.Substring(0, 4) + "-" + xNroRem.Substring(4);
            }
            return xNroRem;
        }
        public string FormatoCAPNRO(string xReintCAP, string xReintNro)
        {
            string xReintegro;

            //EJ [ReintCAP] = 10 y [ReintNRO]= 2306 entonces vReintegro = 10002306 Ver con Jonna
            xReintegro = xReintCAP.ToString() + xReintNro.PadLeft(6, '0');

            return xReintegro;
        }

        public string FormatoFecha(string xDate, String xFormat, bool xTime)
        {
            string xDia = "";
            string xMes = "";
            string xAnio = "";

            if (xDate == null || xDate == "" || xDate == "01/01/1900 0:00:00")
            {
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
            }
            if (xTime == true)
            {
                xDate = xDate + " 00:00:00";
            }
            return xDate;
        }

        public string FormatoFechaBase(string xDate, String xFormat, bool xTime)
        {
            if (xDate == null || xDate == "" || xDate == "01/01/1900 0:00:00")
            {
                return xDate;
            }

            DateTime date = Convert.ToDateTime(xDate);

            int Year = date.Year;
            int Month = date.Month;
            int Day = date.Day;

            string xAnio = Year.ToString();
            string xMes = Month.ToString();
            string xDia = Day.ToString();

            if (Day < 10)
            {
                xDia = "0" + xDia;
            }
            if (Month < 10)
            {
                xMes = "0" + xMes;
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
            }

            string hora = date.Hour.ToString();
            string min = date.Minute.ToString();
            string sec = date.Second.ToString();
            string mili = date.Millisecond.ToString();

            if (hora.Length < 2)
            {
                hora = "0" + hora;
            }

            if (min.Length < 2)
            {
                min = "0" + min;
            }

            if (sec.Length < 2)
            {
                sec = "0" + sec;
            }

            if(mili.Length <2)
            {
                mili = "00" + mili;
            }
            else if (mili.Length < 3)
            {
                mili = "0" + mili;
            }

            xDate = xDate + " " + hora + ":" + min + ":" + sec + "." + mili;

            return xDate;
        }

        Boolean CompararFechas(string xFd, string xFh)
        {
            try
            {
                DateTime date1;
                DateTime date2;
                bool bdate1 = DateTime.TryParse(xFd, out date1);
                bool bdate2 = DateTime.TryParse(xFh, out date2);

                if (bdate1 == false || bdate2 == false)
                    return false;

                int result = DateTime.Compare(date1, date2);
                //string relationship;

                if (result < 0)
                    // relationship = "is earlier than";
                    return true;
                else if (result == 0)
                    //relationship = "is the same time as";
                    return true;
                else
                    //relationship = "is later than";
                    return false;
            }catch (Exception ex){
                return false;
            }
        }
      
        public string FormatoFechaSinBarra(string xDate, String xFormat, bool xTime)
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
                case "mmddyyyy":
                    xDate = xMes + xDia + xAnio;
                    break;
                case "ddmmyyyy":
                    xDate = xDia + xMes + xAnio;
                    break;
                case "yyyymmdd":
                    xDate = xAnio + xMes + xDia;
                    break;
            }

            if (xTime == true)
            {
                xDate = xDate + " 00:00:00";
            }

            return xDate;
        }

        public string buscar_motivos_lista(string xtipomotivo, string xModulo, string xBase, string xUserLog, string xFilialLog) // SE PUEDE BORRAR
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xTablaMotivos = "";
            string bMotivos = "";

            switch (xtipomotivo)
            {
                case "ANULACION":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSANULACION";
                    break;
                case "AJUSTE INGRESO":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSAJUSTEINGRESO";
                    break;
                case "CERRADO":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSSINGESTION";
                    break;
                case "SIN EMPADRONAR":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSSINEMPADRONAR";
                    break;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "Select * from " + xTablaMotivos + " WITH(NOLOCK) Order By CODANUL Desc";

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
                            bMotivos = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                bMotivos = bMotivos + "{";
                                bMotivos = bMotivos + '"' + "CodAnul" + '"' + ":" + '"' + dt.Rows[i]["CODANUL"].ToString() + '"' + ",";
                                bMotivos = bMotivos + '"' + "DescLarga" + '"' + ":" + '"' + dt.Rows[i]["DESCANUL"].ToString() + '"' + ",";
                                bMotivos = bMotivos + '"' + "DescCorta" + '"' + ":" + '"' + dt.Rows[i]["DESC2ANUL"].ToString() + '"' + ",";
                                bMotivos = bMotivos + '"' + "Estado" + '"' + ":" + '"' + dt.Rows[i]["ESTADO"].ToString() + '"' + ",";
                                bMotivos = bMotivos + '"' + "tipomotivo" + '"' + ":" + '"' + xtipomotivo + '"';

                                if (i < Contador - 1)
                                {
                                    bMotivos = bMotivos + "},";
                                }
                                else
                                {
                                    bMotivos = bMotivos + "}";
                                };
                            }
                            bMotivos = bMotivos + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "MOTIVOS: NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            bMotivos = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        bMotivos = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            bMotivos = bMotivos + "{";
                            bMotivos = bMotivos + '"' + "ERROR EN MOTIVOS:" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                bMotivos = bMotivos + "}";
                            }
                            else
                            {
                                bMotivos = bMotivos + "},";
                            };
                        }
                        bMotivos = bMotivos + "]";
                    }
                    con.Close();
                    return bMotivos;

                }
            }

        }
        public string UltimoNroId(string xTabla, string xCampo,string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string idHab;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "";
                sSQL = sSQL + "SELECT MAX(" + xCampo + ")+1 AS MAXIMO FROM " + xTabla + " GRAL_HABILITACIONES WITH(NOLOCK) ";

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
                            idHab = (dt.Rows[0]["MAXIMO"].ToString());
                        }
                        else
                        { idHab = "1"; }
                    }
                    catch (Exception ex)
                    {
                        idHab = "1";
                        con.Close();
                    }
                    return idHab;
                }
            }
        }

        string MenuABM.mod_motivos(string xcodanul, string xdesclarg, string xdescorta, string xestado, string xtipomotivo, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xTablaMotivos = "";
            string bMotivos = "";
            string xReturn = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            xMensaje = ValidarTipoMotivo(xtipomotivo);
            if (xMensaje != "" || EsStrNuloBlanco(xdesclarg) == true || EsStrNuloBlanco(xdescorta) == true || EsStrNuloBlanco(xestado) == true || xdesclarg.Length > 50 || xdescorta.Length > 50
                || (xestado != "A" && xestado != "I"))
            {
                return xMensaje = "Su petición no puede realizarse.";
            }

            switch (xtipomotivo)
            {
                case "ANULACION":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSANULACION";
                    break;
                case "AJUSTE INGRESO":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSAJUSTEINGRESO";
                    break;
                case "CERRADO":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSSINGESTION";
                    break;
                case "SIN EMPADRONAR":
                    xTablaMotivos = "dbo.GRAL_MOTIVOSSINEMPADRONAR";
                    break;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "UPDATE " + xTablaMotivos + " SET ";
                sSql = sSql + "DESCANUL='" + xdesclarg + "',";
                sSql = sSql + "DESC2ANUL='" + xdescorta + "',";
                sSql = sSql + "ESTADO='" + xestado + "'";
                sSql = sSql + "WHERE CODANUL=" + xcodanul;

                //Comienzo la transacción...

                con.Open();
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("ModifMotivosTransaction");
                try
                {

                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    transaction.Commit();

                    xReturn = "1";
                }
                catch (Exception ex)
                {

                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("ModifMotivosTransaction");
                    xReturn = "0";

                }
                con.Close();
                // Regenero la Lista con el nuevo registro...
                //declaro un Objeto refereciando al metodo que necesito para que no de error:
                //"Error 43 - Se requiere una referencia de objeto para el campo, método o propiedad no estáticos"
                WS2 objFuncionesGrales = new WS2();
                bMotivos = objFuncionesGrales.buscar_motivos_lista(xtipomotivo, xModulo, xBase, xUserLog, xFilialLog);
                return bMotivos;

            }
        }
        //ABM MEDICAMENTOS-------------------------------------------------------------------
        string MenuABM.tipo_medicamento(string xModulo, string xBase, string xUserLog, string xFilialLog
)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string cargaMED = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT DISTINCT TIPOMEDIC FROM GRAL_MEDICAMENTOS  WITH(NOLOCK) ORDER BY TIPOMEDIC";

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
                            cargaMED = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                cargaMED = cargaMED + "{";
                                cargaMED = cargaMED + '"' + "tipomedicamento" + '"' + ":" + '"' + dt.Rows[i]["TIPOMEDIC"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    cargaMED = cargaMED + "},";
                                }
                                else
                                {
                                    cargaMED = cargaMED + "}";
                                };
                            }
                            cargaMED = cargaMED + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "CARGA MED:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            cargaMED = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        cargaMED = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            cargaMED = cargaMED + "{";
                            cargaMED = cargaMED + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                cargaMED = cargaMED + "}";
                            }
                            else
                            {
                                cargaMED = cargaMED + "},";
                            };
                        }
                        cargaMED = cargaMED + "]";
                    }
                    con.Close();
                    return cargaMED;

                }
            }
        }
        string MenuABM.filtrar_grillamedic(string xcodmed, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            bool AgregarHabNueva = false;
            SqlDataAdapter ad = new SqlDataAdapter();

            string xNombrePrestador = ""; //xApellido.Trim() + " " + xNombre.Trim();
            string ID = xcodmed;
            string GridMed = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                if (xcodmed != null && xcodmed != "")
                {
                    sSql = "SELECT CODMEDIC, DESCMEDIC, CODCONC, ESTMEDIC,CODMEDIC_LONG, TIPOMEDIC, COMENTARIO FROM dbo.GRAL_MEDICAMENTOS  WITH(NOLOCK)";
                    sSql = sSql + " WHERE CODCONC IN (SELECT CODCONC FROM GRAL_CONCEPTOS  WITH(NOLOCK) WHERE ESTCONC='A')";
                    sSql = sSql + " AND rtrim(ltrim(dbo.GRAL_MEDICAMENTOS.CODMEDIC)) = '" + xcodmed + "'";
                    sSql = sSql + " ORDER BY CODMEDIC";
                }
                else
                {
                    sSql = "SELECT TOP 10 CODMEDIC, DESCMEDIC, CODCONC, ESTMEDIC,CODMEDIC_LONG, TIPOMEDIC, COMENTARIO FROM dbo.GRAL_MEDICAMENTOS  WITH(NOLOCK)";
                    sSql = sSql + " WHERE CODCONC IN (SELECT CODCONC FROM GRAL_CONCEPTOS  WITH(NOLOCK) WHERE ESTCONC='A')";
                    sSql = sSql + " ORDER BY CODMEDIC";
                }

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
                            GridMed = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                GridMed = GridMed + "{";
                                GridMed = GridMed + '"' + "Codmed" + '"' + ":" + '"' + dt.Rows[i]["CODMEDIC"].ToString() + '"' + ",";
                                GridMed = GridMed + '"' + "descmed" + '"' + ":" + '"' + dt.Rows[i]["DESCMEDIC"].ToString() + '"' + ",";
                                GridMed = GridMed + '"' + "Codconc" + '"' + ":" + '"' + dt.Rows[i]["CODCONC"].ToString() + '"' + ",";
                                GridMed = GridMed + '"' + "estmed" + '"' + ":" + '"' + dt.Rows[i]["ESTMEDIC"].ToString() + '"' + ",";
                                GridMed = GridMed + '"' + "codlargo" + '"' + ":" + '"' + dt.Rows[i]["CODMEDIC_LONG"].ToString() + '"' + ",";
                                GridMed = GridMed + '"' + "tipomedic" + '"' + ":" + '"' + dt.Rows[i]["TIPOMEDIC"].ToString() + '"' + ",";
                                GridMed = GridMed + '"' + "comentario" + '"' + ":" + '"' + dt.Rows[i]["COMENTARIO"].ToString() + '"';


                                if (i < Contador - 1)
                                {
                                    GridMed = GridMed + "},";
                                }
                                else
                                {
                                    GridMed = GridMed + "}";
                                };
                            }
                            GridMed = GridMed + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            GridMed = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        GridMed = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            GridMed = GridMed + "{";
                            GridMed = GridMed + '"' + "ERROR GRILLA MEDICOS" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                GridMed = GridMed + "}";
                            }
                            else
                            {
                                GridMed = GridMed + "},";
                            };
                        }
                        GridMed = GridMed + "]";
                    }
                    con.Close();
                    return GridMed;

                }
            }

        }
        string MenuABM.buscar_medicamentos(string xcodmed, string xcodconc, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            bool AgregarHabNueva = false;
            SqlDataAdapter ad = new SqlDataAdapter();

            string xNombrePrestador = ""; //xApellido.Trim() + " " + xNombre.Trim();
            string ID = xcodmed;
            string BusMed = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = " SELECT TIPOMEDIC,COMENTARIO,ESTMEDIC,CODMEDIC, DESCMEDIC, CODCONC, ESTMEDIC, CODMEDIC_LONG  FROM dbo.GRAL_MEDICAMENTOS  WITH(NOLOCK)";
                sSql = sSql + " WHERE rtrim(ltrim(CODMEDIC)) = '" + xcodmed + "' AND CODCONC='" + xcodconc + "'";
                sSql = sSql + " ORDER BY CODMEDIC";

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
                            BusMed = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                BusMed = BusMed + "{";
                                BusMed = BusMed + '"' + "tipomed" + '"' + ":" + '"' + dt.Rows[i]["TIPOMEDIC"].ToString() + '"' + ",";
                                BusMed = BusMed + '"' + "comentario" + '"' + ":" + '"' + dt.Rows[i]["COMENTARIO"].ToString() + '"' + ",";
                                BusMed = BusMed + '"' + "estmed" + '"' + ":" + '"' + dt.Rows[i]["ESTMEDIC"].ToString() + '"' + ",";
                                BusMed = BusMed + '"' + "Codmed" + '"' + ":" + '"' + dt.Rows[i]["CODMEDIC"].ToString() + '"' + ",";
                                BusMed = BusMed + '"' + "descmed" + '"' + ":" + '"' + dt.Rows[i]["DESCMEDIC"].ToString() + '"' + ",";
                                BusMed = BusMed + '"' + "Codconc" + '"' + ":" + '"' + dt.Rows[i]["CODCONC"].ToString() + '"' + ",";
                                BusMed = BusMed + '"' + "codlargo" + '"' + ":" + '"' + dt.Rows[i]["CODMEDIC_LONG"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    BusMed = BusMed + "},";
                                }
                                else
                                {
                                    BusMed = BusMed + "}";
                                };
                            }
                            BusMed = BusMed + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            BusMed = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        BusMed = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            BusMed = BusMed + "{";
                            BusMed = BusMed + '"' + "ERROR BUSQUEDA MEDICOS" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                BusMed = BusMed + "}";
                            }
                            else
                            {
                                BusMed = BusMed + "},";
                            };
                        }
                        BusMed = BusMed + "]";
                    }
                    con.Close();
                    return BusMed;

                }
            }

        }
        string MenuABM.ver_tope(string xcodmed, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            bool AgregarHabNueva = false;
            SqlDataAdapter ad = new SqlDataAdapter();

            string xNombrePrestador = "";
            string ID = xcodmed;
            string VerTope = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "Select * from GRAL_MEDICAMENTOS_TOPES  WITH(NOLOCK) ";
                sSql = sSql + " WHERE rtrim(ltrim(CODMEDIC)) = '" + xcodmed + "'";

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
                            VerTope = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                VerTope = VerTope + "{";
                                VerTope = VerTope + '"' + "tipoIDtopemed" + '"' + ":" + '"' + dt.Rows[i]["ID"].ToString() + '"' + ",";
                                VerTope = VerTope + '"' + "importe" + '"' + ":" + '"' + dt.Rows[i]["tope"].ToString() + '"' + ",";
                                VerTope = VerTope + '"' + "fedesde" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FEDESDE"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                VerTope = VerTope + '"' + "fehasta" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FEHASTA"].ToString(), "dd/mm/yyyy", false) + '"';

                                if (i < Contador - 1)
                                {
                                    VerTope = VerTope + "},";
                                }
                                else
                                {
                                    VerTope = VerTope + "}";
                                };
                            }
                            VerTope = VerTope + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            VerTope = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        VerTope = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            VerTope = VerTope + "{";
                            VerTope = VerTope + '"' + "ERROR TOPES" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                VerTope = VerTope + "}";
                            }
                            else
                            {
                                VerTope = VerTope + "},";
                            };
                        }
                        VerTope = VerTope + "]";
                    }
                    con.Close();
                    return VerTope;

                }
            }

        }
        bool EsValAlfaNumerico(string xValor)
        {
            
            long number1 = 0;
            bool canConvert = long.TryParse(xValor, out number1);
            if (canConvert == true)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        bool EsValBooleano(string xValor)
        {
            bool number1 = false;
            bool canConvert = bool.TryParse(xValor, out number1);
                       
            if (canConvert==true) 
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool EsValNumerico(string xValor)
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
        string ExistePlan(string xPlan, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string sSQL;
            string xMensaje = "El plan " + xPlan + "no es correcto.";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "SELECT isnull(count (cod_plan),0) AS cantidad FROM GRAL_PLANES WHERE Descripcion_plan = '" + xPlan + "'";

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
                            xMensaje = "";
                        }
                        else
                        { xMensaje = "El plan " + xPlan + "no es correcto."; }
                    }
                    catch (Exception ex)
                    {
                        xMensaje = "El plan " + xPlan + "no es correcto.";
                        con.Close();
                    }
                    return xMensaje;
                }
            }
        }
        string ExisteFilDebCap(string xFildeb, string xCap, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string sSQL;
            string xMensaje = "La Filial y CAP " + xFildeb + " y " + xCap + "no son correctos";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "Select isnull(count (CODCAP),0) AS cantidad FROM GRAL_CAPS WHERE FILIAL ='" + xFildeb + "' AND CODCAP = '" + xCap + "'";


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
                            xMensaje = "";
                        }
                        else
                        { xMensaje = "La Filial y CAP " + xFildeb + " y " + xCap + "no son correctos"; }
                    }
                    catch (Exception ex)
                    {
                        xMensaje = "La Filial y CAP " + xFildeb + " y " + xCap + "no son correctos";
                        con.Close();
                    }
                    return xMensaje;
                }
            }
        }

        string ExisteOs(string xOs, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            string sSQL;
            string xMensaje = "La Obra Social " + xOs + " no es correcta";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "Select Count (DESCOS) AS cantidad FROM dbo.GRAL_OBRASSOCIALES WHERE CODOS = '" + xOs + "'";

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
                            xMensaje = "";
                        }
                        else
                        { xMensaje = "La Obra Social " + xOs + " no es correcta"; }
                    }
                    catch (Exception ex)
                    {
                        xMensaje = "La Obra Social " + xOs + " no es correcta";
                        con.Close();
                    }
                    return xMensaje;
                }
            }
        }

        bool validarEspecialidad(string xEspecialidad, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            bool idPto = false;
            string sSQL = "";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "SELECT * FROM GRAL_ESPECIALIDADDISCA WITH(NOLOCK) ";
                sSQL = sSQL + " WHERE ESTESP ='A' AND CODESP = '" + xEspecialidad + "'";

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
                            idPto = true;
                        }
                        else
                        { 
                            idPto = false; 
                        }
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
        bool ValidarTipomed(string xValor, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            bool idPto;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "select distinct tipomedic from gral_medicamentos with(nolock) where tipomedic='" + xValor + "'";

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
        string MenuABM.abm_medicamentos(string xcodmed, string xcodconc, string xdescmed, string xobs, string xtipomed, string xest, string xcodlarg, string modCon, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            bool existeMed = false;
            string bdcodlargo = "";
            string Respcodlargo = "NO";
            bool SehizoInsert = false;
            bool SehizoUpdate = false;
            bool SehizoUpdate2 = false;
           
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return "Su petici&oacute;n no puede realizarse.";
            }

            //---------------
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = " SELECT CODMEDIC, DESCMEDIC, CODCONC, ESTMEDIC, TIPOMEDIC, COMENTARIO, CODMEDIC_LONG ";
                sSql = sSql + "FROM GRAL_MEDICAMENTOS  WITH(NOLOCK) ";
                sSql = sSql + " WHERE rtrim(ltrim(CODMEDIC)) = '" + xcodmed + "' AND CODCONC='" + xcodconc + "'";
                sSql = sSql + " ORDER BY CODMEDIC";

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
                            existeMed = true;
                        }
                        else
                        {
                            existeMed = false;
                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close(); 
                        return "Su petici&oacute;n no puede realizarse.";
                    }

                    con.Close();
                    //---------------------------------------------------------------------------------

                   sSql = "SELECT CODMEDIC, DESCMEDIC, CODCONC, ESTMEDIC, TIPOMEDIC, COMENTARIO ";
                   sSql = sSql + "FROM GRAL_MEDICAMENTOS WITH(NOLOCK) ";
                   sSql = sSql + "WHERE CODMEDIC <> '" + xcodmed + "' AND CODMEDIC_LONG = '" + xcodlarg + "' ";
                   sSql = sSql + "AND CODMEDIC_LONG  <> '' ORDER BY CODMEDIC";
                    
                    using (SqlCommand cmd2 = new SqlCommand(sSql, con))
                    {
                        try
                        {
                            con.Open();
                            SqlDataAdapter da = new SqlDataAdapter(cmd2);
                            dt.Clear();
                            da.Fill(dt);

                            int Contador = dt.Rows.Count;
                            if (dt.Rows.Count > 0)
                            {
                                con.Close(); 
                                return "El c&oacute;digo largo: " + xcodlarg + ", ya es usado por otro medicamento.";
                            }
                            con.Close(); 
                        }
                        catch (SqlException ex)
                        {
                            con.Close(); 
                            return "Su petici&oacute;n no puede realizarse.";
                        }
                    }

                    using (SqlCommand cmd3 = new SqlCommand(sSql, con))
                        {
                            sSql = "SELECT  DISTINCT CODMEDIC_LONG";
                            sSql = sSql + " FROM GRAL_MEDICAMENTOS  WITH(NOLOCK)";
                            sSql = sSql + " WHERE CODMEDIC = '" + xcodmed + "'";

                            try
                            {
                                con.Open();
                                SqlDataAdapter da = new SqlDataAdapter(cmd3);
                                dt.Clear();
                                da.Fill(dt);


                                int Contador = dt.Rows.Count;
                                if (dt.Rows.Count > 0)
                                {
                                    bdcodlargo = dt.Rows[0]["CODMEDIC_LONG"].ToString();

                                    if (bdcodlargo != xcodlarg)
                                    {
                                        if (modCon != "")
                                        {
                                            con.Close(); 
                                            return "P";
                                        }
                                    }
                                }
                                else
                                {
                                    bdcodlargo = "";
                                }
                            }
                            catch (SqlException ex)
                            {
                                con.Close(); 
                                return "Su petici&oacute;n no puede realizarse.";
                            }

                            con.Close();
                        }

                    if (existeMed == false) 
                    {
                        sSql = "INSERT INTO GRAL_MEDICAMENTOS ";
                        sSql = sSql + "(CODMEDIC, CODCONC, DESCMEDIC, COMENTARIO, TIPOMEDIC, ESTMEDIC, IMPORTADO, MODIMPORT, CODMEDIC_LONG) ";
                        sSql = sSql + " VALUES ('" + xcodmed + "','" + xcodconc + "','" + xdescmed + "','" + xobs + "','" + xtipomed + "','" + xest + "','NO','NO','" + xcodlarg + "')";

                        con.Open();
                        SqlCommand command = con.CreateCommand();
                        SqlTransaction transaction;

                        transaction = con.BeginTransaction("AltaGralMedTransaction");
                        try
                        {
                            command.Connection = con;
                            command.Transaction = transaction;

                            command.CommandText = sSql;
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            SehizoInsert = true;
                        }
                        catch (Exception ex)
                        {
                            con.Close(); 
                            return "Su petici&oacute;n no puede realizarse.";
                        }

                        if (modCon == "SI")
                        {
                            sSql = "UPDATE GRAL_MEDICAMENTOS SET CODMEDIC_LONG = '" + xcodlarg + "'";
                            sSql = sSql + " WHERE CODMEDIC = '" + xcodmed + "'";

                            transaction = con.BeginTransaction("AltaGralMedTransaction");
                            try
                            {
                                command.Connection = con;
                                command.Transaction = transaction;

                                command.CommandText = sSql;
                                command.ExecuteNonQuery();
                                transaction.Commit();
                                SehizoUpdate = true;
                                con.Close(); 
                                return "El medicamento fue dado de alta y se ha modificado el código largo para todos los registros.";
                            }
                            catch (Exception ex)
                            {
                                con.Close(); 
                                return "Su petici&oacute;n no puede realizarse.";
                            }
                        }
                        else
                        {
                            con.Close(); 
                            return "El medicamento fue dado de alta.";
                        }
                    }
                    else
                    {
                            sSql = "UPDATE GRAL_MEDICAMENTOS ";
                            sSql = sSql + "SET DESCMEDIC='" + xdescmed + "', ";
                            sSql = sSql + "COMENTARIO='" + xobs + "', ";
                            sSql = sSql + "TIPOMEDIC='" + xtipomed + "', ";
                            sSql = sSql + "ESTMEDIC='" + xest + "', ";
                            sSql = sSql + "IMPORTADO='NO', ";
                            sSql = sSql + "MODIMPORT='NO' ";
                            sSql = sSql + "WHERE CODMEDIC = '" + xcodmed + "' AND CODCONC = '" + xcodconc + "'";

                            con.Open();
                            SqlCommand command = con.CreateCommand();
                            SqlTransaction transaction;

                            transaction = con.BeginTransaction("AltaGralMedTransaction");

                            try
                            {
                                command.Connection = con;
                                command.Transaction = transaction;

                                command.CommandText = sSql;
                                command.ExecuteNonQuery();
                                SehizoUpdate = true;
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback("AltaGralMedTransaction");
                                con.Close(); 
                                return "Su petici&oacute;n no puede realizarse.";
                            }

                            if (modCon == "SI")
                            {
                                sSql = "UPDATE GRAL_MEDICAMENTOS SET CODMEDIC_LONG = '" + xcodlarg + "'";
                                sSql = sSql + " WHERE CODMEDIC = '" + xcodmed + "'";

                                try
                                {
                                    command.CommandText = sSql;
                                    command.ExecuteNonQuery();
                                    transaction.Commit();
                                    SehizoUpdate2 = true;
                                    con.Close(); 
                                    return "El medicamento fue modificado con &eacute;xito.";
                                }
                                catch (Exception ex)
                                {
                                    con.Close(); 
                                    return "Su petici&oacute;n no puede realizarse.";
                                }
                            }
                            else
                            {
                                con.Close(); 
                                return "El medicamento fue modificado EXCEPTO EL C&Oacute;DIGO LARGO.";
                            } 
                    }
                }
            }
        }
        bool ExisteCodConYCodMed(string xcodconc, string xcodmed, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            if (xcodconc == null || xcodmed == null)
            {
                return false;

            }
            int Cant1 = 0;
            int Cant2 = 0;
            string sSQL;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = "SELECT isnull(COUNT(*),0) AS CANTIDAD1 ";
                sSQL = sSQL + " FROM GRAL_MEDICAMENTOS  WITH(NOLOCK) ";
                sSQL = sSQL + " WHERE  CODMEDIC ='" + xcodmed + "'";

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

                    sSQL = "SELECT isnull(COUNT(*),0) AS CANTIDAD2 ";
                    sSQL = sSQL + " FROM GRAL_CONCEPTOS  WITH(NOLOCK) ";
                    sSQL = sSQL + " WHERE  CODCONC ='" + xcodconc + "'";

                    using (SqlCommand cmd2 = new SqlCommand(sSQL, con))
                    {
                        try
                        {

                            DataTable dt = new DataTable();
                            SqlDataAdapter da = new SqlDataAdapter(cmd2);
                            dt.Clear();
                            da.Fill(dt);
                            if (Int32.Parse(dt.Rows[0]["CANTIDAD2"].ToString()) > 0)
                            {
                                Cant2 = Int32.Parse(dt.Rows[0]["CANTIDAD2"].ToString());
                            }

                        }
                        catch (Exception ex)
                        {
                            con.Close();
                            return false;
                        }

                        con.Close();

                        if (Cant1 == 0 || Cant2 == 0)
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
        }
        string ValidaPeriodoCabecera(string xNroAfiliado, string xCodconc, string xFdesde, string xFhasta, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            bool xError = false;
            DateTime feDesdeAlta = DateTime.Parse(xFdesde);
            DateTime feHastaAlta = DateTime.Parse(xFhasta);

            if (feDesdeAlta >= feHastaAlta)
            {
                xError = true;
            }

            string xFilafil = xNroAfiliado.Substring(0, 2);
            string xnroAfil = xNroAfiliado.Substring(2, 7);
            string xBeneafil = xNroAfiliado.Substring(9, 2);

            string sql = "";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sql = "SELECT FEDESDE,FEHASTA FROM CABECERAS WITH(NOLOCK)  ";
                sql = sql + " WHERE FILAFIL = '" + xFilafil + "' AND NROAFIL = '" + xnroAfil + "'";
                sql = sql + " AND BENEFAFIL = '" + xBeneafil + "' AND CODCONC ='" + xCodconc + "'";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;

                        if (Contador > 0)
                        {
                            for (int i = 0; i < Contador; i++)
                            {
                                DateTime desde = DateTime.Parse(dt.Rows[i]["fedesde"].ToString());
                                DateTime hasta = DateTime.Parse(dt.Rows[i]["fehasta"].ToString());

                                if (desde <= feDesdeAlta && hasta >= feDesdeAlta)
                                {
                                    xError = true;
                                }
                                if (desde <= feHastaAlta && hasta >= feHastaAlta)
                                {
                                    xError = true;
                                }
                                if (desde >= feDesdeAlta && hasta <= feHastaAlta)
                                {
                                    xError = true;
                                }
                                if (desde <= feDesdeAlta && hasta >= feHastaAlta)
                                {
                                    xError = true;
                                }

                                if (xError == true)
                                {
                                    con.Close();
                                    return "El período se superpone con otra cabecera para el mismo socio, imposible continuar.";
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        con.Close();
                        return "Error: " + e.Message;
                    }
                    con.Close();
                }
            }

            return "";
        }
        string ExisteModulo(string xCodconc, string xModcab, string xFdesde, string xFhasta, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            DateTime desd;
            DateTime hast;

            if (xCodconc == "" || xFdesde == "" || xFhasta == "")
            {
                return "Existen campos obligatorios incompletos, imposible continuar.";
            }

            try
            {
                desd = DateTime.Parse(xFdesde);
                hast = DateTime.Parse(xFhasta);

                if (desd > hast)
                {
                    return "Su petición no puede ser ejecutada, fecha desde no puede ser mayor a fecha hasta.";
                }
                hast = hast.AddDays(1);
            }
            catch
            {
                return "Su petición no puede ser ejecutada, fechas invalidas.";
            }

            if (ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog) != "")
            {
                return "Su petición no puede ser ejecutada.";
            }

            string xMensaje = "";
            string sql;


            if (validateModulo(xCodconc, xFdesde, xFhasta, xModulo, xBase, xUserLog, xFilialLog) == "true")
            {
                if (xModcab == "" || xModcab == null || xModcab == "NULL")
                {
                    return "Existen campos obligatorios incompletos, imposible continuar";
                }
                else
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
                    {

                        con.Open();

                        sql = "SELECT COUNT(*) AS CANT FROM GRAL_MODULOS WITH(NOLOCK) ";
                        sql += "WHERE CODCONC='" + xCodconc + "' AND MODULO = '" + xModcab + "' AND ((FEDESDE >= '" + FormatoFechaBase(desd.ToString(), "yyyy/mm/dd", true) + "' ";                        
                        sql += "AND FEDESDE <= '" + FormatoFechaBase(hast.ToString(), "yyyy/mm/dd", true) + "') OR (FEHASTA >= '" + FormatoFechaBase(desd.ToString(), "yyyy/mm/dd", true) + "' ";
                        sql += "AND FEHASTA <= '" + FormatoFechaBase(hast.ToString(), "yyyy/mm/dd", true) + "') OR (FEDESDE < '" + FormatoFechaBase(desd.ToString(), "yyyy/mm/dd", true) + "' ";
                        sql += "AND FEHASTA > '" + FormatoFechaBase(hast.ToString(), "yyyy/mm/dd", true) + "'))";

                        using (SqlCommand cmd1 = new SqlCommand(sql, con))
                        {
                            DataTable dt1 = new DataTable();
                            SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                            da1.Fill(dt1);
                            if (Int32.Parse(dt1.Rows[0]["CANT"].ToString()) == 0)
                            {
                                xMensaje = "No existe el Modulo para el período seleccionado.";
                            }
                        }
                        con.Close();
                    }
                }
            }

            return xMensaje;
        }

        public string validateModulo(string xCodconc, string xFdesde, string xFhasta, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            DateTime desd;
            DateTime hast;

            if (xCodconc == "" || xFdesde == "" || xFhasta == "")
            {
                return "Su petición no puede ser ejecutada.";
            }

            try
            {
                desd = DateTime.Parse(xFdesde);
                hast = DateTime.Parse(xFhasta);

                if (desd > hast)
                {
                    return "Su petición no puede ser ejecutada, fecha desde no puede ser mayor a fecha hasta.";
                }
                hast = hast.AddDays(1);
            }
            catch
            {
                return "Su petición no puede ser ejecutada, fechas invalidas.";
            }

            if (ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog) != "")
            {
                return "Su petición no puede ser ejecutada.";
            }

            string xMensaje = "";
            string sql;

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
                {
                    sql = "SELECT COUNT(MODULO) AS CANT FROM GRAL_MODULOS WITH(NOLOCK) ";
                    sql += "WHERE CODCONC='" + xCodconc + "' AND ((FEDESDE >= '" + FormatoFechaBase(desd.ToString(), "yyyy/mm/dd", true) + "' ";
                    sql += "AND FEDESDE <= '" + FormatoFechaBase(hast.ToString(), "yyyy/mm/dd", true) + "') OR (FEHASTA >= '" + FormatoFechaBase(desd.ToString(), "yyyy/mm/dd", true) + "' ";
                    sql += "AND FEHASTA <= '" + FormatoFechaBase(hast.ToString(), "yyyy/mm/dd", true) + "') OR (FEDESDE < '" + FormatoFechaBase(desd.ToString(), "yyyy/mm/dd", true) + "' ";
                    sql += "AND FEHASTA > '" + FormatoFechaBase(hast.ToString(), "yyyy/mm/dd", true) + "'))";


                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        con.Open();
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        if (Int32.Parse(dt.Rows[0]["CANT"].ToString()) > 0)
                        {
                            xMensaje = "true";
                        }
                        else
                        {
                            xMensaje = "false";
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception e)
            {
                xMensaje = e.Message;
            }

            return xMensaje;
        }

        bool ExisteCodCon(string xcodconc, string xModulo, string xBase, string xUserLog, string xFilialLog)
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
        bool ValidarImporte2Dec(string ximporte)
        {

            int posicionComa = 0;
            int LargoTotal = ximporte.Length;
            // 1 .- Busco la posicion de la coma... 
            for (int i = 0; i < LargoTotal; i++)
            {
                if (ximporte.ToString().Substring(i, 1) == ",")
                {
                    posicionComa = i + 1;
                    break;
                }
            }

            // 2.- Si no hay coma, el importe es un entero, entonces salgo...
            if (posicionComa == 0)
            {
                return true;
            }
            // 3.- Calculo la cantidad de decimales y procuro que no sean mas de dos....
            int CantDecimales = LargoTotal - posicionComa;
            string LuegoDeLaComa = ximporte.ToString().Substring(posicionComa, CantDecimales);

            if (LuegoDeLaComa.Length <= 2)
            { return true; }
            else
            { return false; }

        }
        bool EsFechaValida(string xFecha)
        {
            DateTime dFecha;
            bool bFecha;
            if (xFecha == null)
            {
                return false;
            }
            else
            {
                bFecha = DateTime.TryParse(xFecha, out dFecha);
                //Valido que los valores booleanos sean true o sea que se pudo convertir bien la fecha...
                if (bFecha == false)
                { return false; }
                else
                {
                    //Valido que el año de las fechas sea mayor a 1900...
                    if (dFecha.Year < 1900)
                    { return false; }
                    else
                    { return true; }
                }
            }
        }

        bool ValidarFdesdeFhasta(string xfedesde, string xfehasta)
        {
            DateTime dFechaDesde, dFechaHasta;
            bool bFechaDesde;
            bool bFechaHasta;
            if (xfedesde == null)
            {
                return true;
            }
            else
            {
                if (xfehasta == null)
                {
                    return true;
                }
                else
                {
                    bFechaDesde = DateTime.TryParse(xfedesde, out dFechaDesde);
                    bFechaHasta = DateTime.TryParse(xfehasta, out dFechaHasta);

                    //Valido que los valores booleanos sean true o sea que se pudo convertir bien las fechas...
                    if (bFechaDesde == false || bFechaHasta == false)
                    { return true; }

                    //Valido que el año de las fechas sea mayor a 1900...
                    if (dFechaDesde.Year < 1900 || dFechaHasta.Year < 1900)
                    { return true; }

                    // Valido que la fecha desde sea menor que la fecha hasta...
                    if (dFechaDesde > dFechaHasta)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        string MenuABM.abm_topes(string xcodmed, string xcodconc, string ximporte, string xfedesde, string xfehasta, string xIdtope, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            //string xFilialParam = xSC;
            string sSql = "";
            int Contador = 0;
            int xFilasAfectadas = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string MsgTope = "";
            string xReturn = "";
            SqlTransaction transaction;
            if (EsStrNuloBlanco(xcodmed) == true || EsStrNuloBlanco(xcodconc) == true || EsStrNuloBlanco(ximporte) == true
               || EsStrNuloBlanco(xfedesde) == true || EsStrNuloBlanco(xfehasta) == true
               || ExisteCodConYCodMed(xcodconc, xcodmed, xModulo, xBase, xUserLog, xFilialLog) == false
               || ValidarImporte2Dec(ximporte) == false

               )
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su petición no puede realizarse." + '"' + "}]";
                return xMensaje;
            }
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            // Validacion aparte del rango de fechas por Mariano
            if (ValidarFdesdeFhasta(xfedesde, xfehasta) == true)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Fecha Inválida." + '"' + "}]";
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                // Transformo la coma decimal en punto
                ximporte = ximporte.Replace(',', '.');

                // Verifico que los campos esten con valores
                if (xcodmed != null && xfedesde != null && xfehasta != null)
                {

                    xfedesde = FormatoFechaBase(xfedesde, "yyyy/mm/dd", true);

                    xfehasta = FormatoFechaBase(xfehasta, "yyyy/mm/dd", true);



                    if (xIdtope == null || xIdtope == "" || xIdtope == "0") // Es un alta...
                    {
                        // Validacion por superposicion de fechas entre topes...
                        sSql = "SELECT COUNT(*) AS CANTIDAD ";
                        sSql = sSql + " FROM GRAL_MEDICAMENTOS_TOPES  WITH(NOLOCK) ";
                        sSql = sSql + " WHERE rtrim(ltrim(CODMEDIC))='" + xcodmed + "' AND (('" + xfedesde + "' >= FEDESDE AND '" + xfedesde + "' <= FEHASTA) OR ('" + xfehasta + "' >= FEDESDE AND '" + xfehasta + "' <= FEHASTA))";

                        using (SqlCommand cmd = new SqlCommand(sSql, con))
                        {

                            DataTable dt = new DataTable();

                            con.Open();
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.Fill(dt);

                            Contador = dt.Rows.Count;
                            if (dt.Rows.Count > 0)
                            {


                                if (dt.Rows[0]["CANTIDAD"].ToString() != null && dt.Rows[0]["CANTIDAD"].ToString() != "" && dt.Rows[0]["CANTIDAD"].ToString() != "0")
                                {
                                    MsgTope = "[{'Existe superposición de períodos para el Tope ingresado.'}]";
                                    return MsgTope;
                                }
                                else
                                {
                                    MsgTope = "[]";

                                }
                            }

                        }
                        // Fin Validacion...

                        // Procedo con el Alta.....
                        sSql = "SELECT  (max(isnull(ID,0))+ 1) AS MAXIMO FROM GRAL_MEDICAMENTOS_TOPES  WITH(NOLOCK)";
                        using (SqlCommand cmd2 = new SqlCommand(sSql, con))
                        {
                            DataTable dt2 = new DataTable();
                            //float ximportefloat = float.TryParse(ximporte, out xfloattrue);

                            SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                            da2.Fill(dt2);
                            string xNuevoId = "";
                            Contador = dt2.Rows.Count;
                            if (dt2.Rows.Count > 0)
                            {

                                for (int i = 0; i < dt2.Rows.Count; i++)
                                {
                                    xNuevoId = dt2.Rows[i]["MAXIMO"].ToString();
                                }

                                sSql = "INSERT INTO GRAL_MEDICAMENTOS_TOPES VALUES";
                                sSql = sSql + "(" + xNuevoId + ",'" + xcodmed + "'," + ximporte + ",0,'" + xfedesde + "','" + xfehasta + "','NO','NO')";

                                xReturn = "El Tope se ha dado de Alta.";
                            }
                        }
                    }
                    else // Es Un Update...
                    {
                        sSql = "UPDATE GRAL_MEDICAMENTOS_TOPES ";
                        sSql = sSql + "SET  TOPE = " + ximporte + ",";
                        sSql = sSql + "FEDESDE = '" + xfedesde + "',";
                        sSql = sSql + "FEHASTA = '" + xfehasta + "'";
                        sSql = sSql + " WHERE ID =" + xIdtope;

                        xReturn = "El Tope se ha Modificado.";

                        con.Open();
                    }

                    //Comienzo la transacción...
                    SqlCommand command = con.CreateCommand();
                    transaction = con.BeginTransaction("AltaTopeTransaction");
                    try
                    {
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = sSql;
                        xFilasAfectadas = command.ExecuteNonQuery();
                        transaction.Commit();
                        if (xFilasAfectadas == 0) // Esto es por si es un Update y no actualizase nada...
                        {
                            xReturn = "No se realizo ninguna operacion.";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Algo salio mal, hago roll back de la transaccion.
                        transaction.Rollback("AltaTopeTransaction");
                        xReturn = "No se realizo ninguna operacion.";
                        return xReturn;
                    }

                    con.Close();
                    return xReturn;
                }

                else
                {
                    // Algo salio mal, hago roll back de la transaccion.
                    xReturn = "Se proporcionaron valores vacios. No se realizo ninguna operacion.";
                    return xReturn;
                }

            }
        }

        string MenuABM.cargarRNOS(string xfilAfil, string xnroAfil, string xbeneAfil, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string carRnos = "";

            if (EsStrNuloBlanco(xfilAfil) == true || EsStrNuloBlanco(xnroAfil) == true || EsStrNuloBlanco(xbeneAfil) == true
                || EsValNumerico(xfilAfil) == false || EsValNumerico(xnroAfil) == false || EsValNumerico(xbeneAfil) == false
                || xfilAfil.Length > 2 || xnroAfil.Length > 12 || xbeneAfil.Length > 3
                )
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su petición no puede realizarse." + '"' + "}]";
                return xMensaje;
            }
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }


            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT * FROM GRAL_AFILIADOS_SSS WITH(NOLOCK)";
                sSql = sSql + " WHERE FILAFIL = '" + xfilAfil + "' AND NROAFIL = '" + xnroAfil + "' AND BENEFAFIL = '" + xbeneAfil + "'";


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
                            carRnos = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                //{["Filial":"[FILAFIL]","NroAfiliado":"[NROAFIL]","Benf":"[BENEFAFIL]","Fedesde":"[FEDESDE]","Fehasta":"[FEHASTA]", "rnos":"[RNOS]"]}
                                carRnos = carRnos + "{";
                                carRnos = carRnos + '"' + "Filial" + '"' + ":" + '"' + dt.Rows[i]["FILAFIL"].ToString() + '"' + ",";
                                carRnos = carRnos + '"' + "NroAfiliado" + '"' + ":" + '"' + dt.Rows[i]["NROAFIL"].ToString() + '"' + ",";
                                carRnos = carRnos + '"' + "Benf" + '"' + ":" + '"' + dt.Rows[i]["BENEFAFIL"].ToString() + '"' + ",";
                                carRnos = carRnos + '"' + "Fedesde" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FEDESDE"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                carRnos = carRnos + '"' + "Fehasta" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FEHASTA"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                carRnos = carRnos + '"' + "rnos" + '"' + ":" + '"' + dt.Rows[i]["RNOS"].ToString() + '"';


                                if (i < Contador - 1)
                                {
                                    carRnos = carRnos + "},";
                                }
                                else
                                {
                                    carRnos = carRnos + "}";
                                };
                            }
                            carRnos = carRnos + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "carRnos:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            carRnos = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        carRnos = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            carRnos = carRnos + "{";
                            carRnos = carRnos + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                carRnos = carRnos + "}";
                            }
                            else
                            {
                                carRnos = carRnos + "},";
                            };
                        }
                        carRnos = carRnos + "]";
                    }
                    con.Close();
                    return carRnos;

                }
            }


        }
        bool ValidarCuit(string xCuit)
        {
            bool xLongitudOk = false;
            bool xesNumerico = false;

            //evaluo si el CUIT tine todos sus digitos numericos
            xesNumerico = EsValNumerico(xCuit);

            //Evaluo si el CUIT tiene 11 digitos
            if (xCuit.Length == 11)
            { xLongitudOk = true; }
            else
            {
                xLongitudOk = false;
            }

            if (xesNumerico == true && xLongitudOk == true)
            { return true; }
            else
            { return false; }

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
        string MenuABM.BuscarAfiliadoCab(string xNroAfiliado, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xFilafil, xnroAfil, xBeneafil;
            string bAfilc = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xNroAfiliado) == true ||
                EsValNumerico(xNroAfiliado) == false ||
                xNroAfiliado.Length != 11
               )
            {
                xMensaje = "Su Petición no puede ser ejecutada.";
                return xMensaje;
            }

            xFilafil = xNroAfiliado.Substring(0, 2);
            xnroAfil = xNroAfiliado.Substring(2, 7);
            xBeneafil = xNroAfiliado.Substring(9, 2);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT APELLIDO + ' ' + NOMBRE AS NOMBREAFIL,FILDEBITO,TEL1,TEL2,CAP,PLAN_AFIL ";
                sSql = sSql + " FROM GRAL_AFILIADOS WITH(NOLOCK) ";
                sSql = sSql + "WHERE nroafil='" + xnroAfil + "'" + " AND FILAFIL='" + xFilafil + "' AND BENEFAFIL='" + xBeneafil + "'";

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
                            bAfilc = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                bAfilc = bAfilc + "{";
                                bAfilc = bAfilc + '"' + "Nombre" + '"' + ":" + '"' + dt.Rows[i]["NOMBREAFIL"].ToString() + '"' + ",";
                                bAfilc = bAfilc + '"' + "Fildeb" + '"' + ":" + '"' + dt.Rows[i]["FILDEBITO"].ToString() + '"' + ",";
                                bAfilc = bAfilc + '"' + "Tel" + '"' + ":" + '"' + dt.Rows[i]["TEL1"].ToString() + " " + dt.Rows[i]["TEL2"].ToString() + '"' + ",";
                                bAfilc = bAfilc + '"' + "Cap" + '"' + ":" + '"' + dt.Rows[i]["CAP"].ToString() + '"' + ",";
                                bAfilc = bAfilc + '"' + "Plan" + '"' + ":" + '"' + dt.Rows[i]["PLAN_AFIL"].ToString() + '"';


                                if (i < Contador - 1)
                                {
                                    bAfilc = bAfilc + "},";
                                }
                                else
                                {
                                    bAfilc = bAfilc + "}";
                                };
                            }
                            bAfilc = bAfilc + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "AFILIADOS:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            bAfilc = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        bAfilc = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            bAfilc = bAfilc + "{";
                            bAfilc = bAfilc + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                bAfilc = bAfilc + "}";
                            }
                            else
                            {
                                bAfilc = bAfilc + "},";
                            };
                        }
                        bAfilc = bAfilc + "]";
                    }
                    con.Close();
                    return bAfilc;

                }
            }
        }
        string MenuABM.validateRnosCab(string xNroAfiliado, string xFdesde, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xFilafil, xnroAfil, xBeneafil;
            string vRnos = "";

            //ALFAOSCAR
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xNroAfiliado) == true ||
                EsValNumerico(xNroAfiliado) == false ||
                xNroAfiliado.Length != 11 ||
                EsStrNuloBlanco(xFdesde) == true
               )
            {
                xMensaje = "Su Petición no puede ser ejecutada.";
                return xMensaje;
            }

            xFilafil = xNroAfiliado.Substring(0, 2);
            xnroAfil = xNroAfiliado.Substring(2, 7);
            xBeneafil = xNroAfiliado.Substring(9, 2);
            xFdesde = FormatoFecha(xFdesde, "yyyy/mm/dd", false);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT RNOS FROM GRAL_AFILIADOS_SSS  WITH(NOLOCK)   ";
                sSql = sSql + "WHERE (nroafil='" + xnroAfil + "'" + " AND FILAFIL='" + xFilafil + "' AND BENEFAFIL='" + xBeneafil + "'";
                sSql = sSql + " AND FEDESDE <='" + xFdesde + "' AND   FEHASTA >='" + xFdesde + "')";
                sSql = sSql + " OR (nroafil='" + xnroAfil + "'" + " AND FILAFIL='" + xFilafil + "' AND BENEFAFIL='" + xBeneafil + "'";
                sSql = sSql + " AND FEDESDE <='" + xFdesde + "' AND FEHASTA IS NULL )";

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
                            vRnos = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                vRnos = vRnos + "{";
                                vRnos = vRnos + '"' + "Afiliacion" + '"' + ":" + '"' + dt.Rows[i]["RNOS"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    vRnos = vRnos + "},";
                                }
                                else
                                {
                                    vRnos = vRnos + "}";
                                };
                            }
                            vRnos = vRnos + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "AFILIADOS:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            vRnos = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        vRnos = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            vRnos = vRnos + "{";
                            vRnos = vRnos + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                vRnos = vRnos + "}";
                            }
                            else
                            {
                                vRnos = vRnos + "},";
                            };
                        }
                        vRnos = vRnos + "]";
                    }
                    con.Close();
                    return vRnos;

                }
            }
        }

        string MenuABM.ObtenerDocumentosCab(string xNroAfiliado, string xCodconc, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string xFilafil, xnroAfil, xBeneafil;
            string oDocab = "";
            string xPATOL_INF_RESP = "false";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xNroAfiliado) == true ||
                EsValNumerico(xNroAfiliado) == false ||
                xNroAfiliado.Length != 11 ||
                ExisteCodCon(xCodconc, xModulo, xBase, xUserLog, xFilialLog) == false
               )
            {
                xMensaje = "Su Petición no puede ser ejecutada.";
                return xMensaje;
            }

            xFilafil = xNroAfiliado.Substring(0, 2);
            xnroAfil = xNroAfiliado.Substring(2, 7);
            xBeneafil = xNroAfiliado.Substring(9, 2);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                //----------------------------------------------------------------------------------------

                sSql = "SELECT PATOL_INF_RESP FROM GRAL_CONCEPTOS WITH(NOLOCK) WHERE CODCONC = '" + xCodconc + "'";

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
                            xPATOL_INF_RESP = dt.Rows[0]["PATOL_INF_RESP"].ToString();
                        }

                        else
                        {
                            xPATOL_INF_RESP = "";

                        }
                    }
                    catch (SqlException ex)
                    {
                        xPATOL_INF_RESP = "";
                    }
                    //----------------------------------------------------------------------------------------
                    sSql = "SELECT CAB_DOCUMENTACION.CODDOC, RESP_DOC, DESCDOC ";
                    sSql = sSql + " FROM CAB_DOCUMENTACION  WITH(NOLOCK) ";
                    sSql = sSql + " JOIN GRAL_DOCUMENTACION  WITH(NOLOCK) ON CAB_DOCUMENTACION.CODDOC = GRAL_DOCUMENTACION.CodDoc";
                    sSql = sSql + " WHERE IDCAB IN (SELECT MAX(ID) FROM CABECERAS  WITH(NOLOCK) ";
                    sSql = sSql + " WHERE FILAFIL='" + xFilafil + "' AND NROAFIL='" + xnroAfil + "' AND BENEFAFIL='" + xBeneafil + "' AND ID > 0 AND CODCONC='" + xCodconc + "') ";
                    sSql = sSql + " AND GRAL_DOCUMENTACION.ESTDOC='A' ";
                    sSql = sSql + " Union ";
                    sSql = sSql + " SELECT CODDOC, RESP_DOC_DEFAULT, DESCDOC ";
                    sSql = sSql + " FROM GRAL_DOCUMENTACION  WITH(NOLOCK) ";
                    sSql = sSql + " WHERE (CODCONC ='" + xCodconc + "' OR CODCONC='GENERAL') ";
                    sSql = sSql + " AND ESTDOC = 'A' AND CODDOC NOT IN (SELECT CODDOC FROM CAB_DOCUMENTACION  WITH(NOLOCK) ";
                    sSql = sSql + " WHERE IDCAB IN (SELECT MAX(ID) FROM CABECERAS  WITH(NOLOCK) ";
                    sSql = sSql + " WHERE FILAFIL='" + xFilafil + "' AND NROAFIL='" + xnroAfil + "' AND BENEFAFIL='" + xBeneafil + "' AND ID >0 AND CODCONC='" + xCodconc + "' ))";

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
                                oDocab = "[";
                                for (int i = 0; i < dt2.Rows.Count; i++)
                                {
                                    string resp = "";
                                    if (xPATOL_INF_RESP == "N")
                                    {
                                        resp = "No Aplica";
                                    } else{
                                        if (dt2.Rows[i]["RESP_DOC"].ToString() == "A")
                                        {
                                            resp = "Afiliado";
                                        } else {
                                            resp = "Prestador";
                                        }
                                    }
                                    oDocab = oDocab + "{";
                                    oDocab = oDocab + '"' + "Responsable" + '"' + ":" + '"' + resp + '"' + ",";
                                    oDocab = oDocab + '"' + "CODDOC" + '"' + ":" + '"' + dt2.Rows[i]["coddoc"].ToString() + '"' + ",";
                                    oDocab = oDocab + '"' + "RESPDOC" + '"' + ":" + '"' + dt2.Rows[i]["resp_doc"].ToString() + '"' + ",";
                                    oDocab = oDocab + '"' + "DESCDOC" + '"' + ":" + '"' + dt2.Rows[i]["descdoc"].ToString() + '"';


                                    if (i < Contador - 1)
                                    {
                                        oDocab = oDocab + "},";
                                    }
                                    else
                                    {
                                        oDocab = oDocab + "}";
                                    };
                                }
                                oDocab = oDocab + "]";

                            }

                            else
                            {
                                // STATUS: NOTFOUND
                                objStatus.Status = "AFILIADOS:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                                oDocab = "[]";

                            }
                        }
                        catch (SqlException ex)
                        {
                            string xMesgError;
                            oDocab = "[";
                            foreach (SqlError error in ex.Errors)
                            {
                                xMesgError = error.Message.Replace("\"", "'"); ;
                                oDocab = oDocab + "{";
                                oDocab = oDocab + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                                xContaErr = xContaErr + 1;
                                if (ex.Errors.Count == xContaErr)
                                {
                                    oDocab = oDocab + "}";
                                }
                                else
                                {
                                    oDocab = oDocab + "},";
                                };
                            }
                            oDocab = oDocab + "]";
                        }
                        con.Close();
                        return oDocab;

                    }
                }
            }
        }
        string MenuABM.ObtenerMedicamentosCab(string xCodconc, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string vMed = "";

            //ALFAOSCAR
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xCodconc) == true ||
                ExisteCodCon(xCodconc, xModulo, xBase, xUserLog, xFilialLog) == false

               )
            {
                xMensaje = "Su Petición no puede ser ejecutada.";
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT * FROM GRAL_MEDICAMENTOS WITH(NOLOCK)  ";
                sSql = sSql + " WHERE CODCONC='" + xCodconc + "'";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        int Contador = dt.Rows.Count;
                        vMed = "[";
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            vMed = vMed + "{";
                            vMed = vMed + '"' + "Codigo" + '"' + ":" + '"' + dt.Rows[i]["CODMEDIC"].ToString() + '"' + ",";
                            vMed = vMed + '"' + "Codmediclong" + '"' + ":" + '"' + dt.Rows[i]["CODMEDIC_LONG"].ToString() + '"' + ",";
                            vMed = vMed + '"' + "Medicamento" + '"' + ":" + '"' + dt.Rows[i]["DESCMEDIC"].ToString() + '"';


                            if (i < Contador - 1)
                            {
                                vMed = vMed + "},";
                            }
                            else
                            {
                                vMed = vMed + "}";
                            };

                        }
                        vMed = vMed + "]";
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        vMed = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            vMed = vMed + "{";
                            vMed = vMed + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                vMed = vMed + "}";
                            }
                            else
                            {
                                vMed = vMed + "},";
                            };
                        }
                        vMed = vMed + "]";
                    }
                    con.Close();
                    return vMed;

                }
            }
        }
        public string UltimoNroIdCabeceras(string xModulo, string xBase, string xUserLogstring,string xFilialLog)
        {
            string idHab = "";         
            string sSQL = "";
            long auxiliar1 = 0;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSQL = sSQL + "SELECT MAX(ID) AS MAXIMO FROM CABECERAS WITH(NOLOCK) WHERE  ID >= " + xFilialLog + "00000001 AND ID <= " + xFilialLog + "99999999";
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
                            if (dt.Rows[0]["MAXIMO"].ToString() != "" && dt.Rows[0]["MAXIMO"].ToString() != null && dt.Rows[0]["MAXIMO"].ToString() != "NULL")
                            {
                                string auxiliar = (dt.Rows[0]["MAXIMO"].ToString());
                                auxiliar1 = long.Parse(auxiliar);
                            }
                            else
                            {
                                string auxiliar = xFilialLog + "00000000";
                                auxiliar1 = long.Parse(auxiliar); 
                            }
                           
                        } else {
                            string auxiliar = xFilialLog + "00000000";
                            auxiliar1 = long.Parse(auxiliar); 
                        }
                      auxiliar1=auxiliar1 + 1;
                      idHab = auxiliar1.ToString();
                    }catch (Exception ex){
                        con.Close();
                    }
                    return idHab;
                }
            }
        }
        public string UltimoNroIdRenglones( string xModulo, string xBase, string xUserLog,string xFilialLog)
        {
            string idHab="";
            string sSQL="";
            long auxiliar = 0;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSQL = sSQL + "SELECT MAX(ID) AS MAXIMO FROM RENGLONES WITH(NOLOCK) WHERE  ID >= " + xFilialLog + "00000001 AND ID <= " + xFilialLog + "99999999";

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
                            if((dt.Rows[0]["MAXIMO"].ToString())!=null && (dt.Rows[0]["MAXIMO"].ToString())!="" ){
                                string auxiliar1 = (dt.Rows[0]["MAXIMO"].ToString());
                                auxiliar = long.Parse(auxiliar1);
                            } else {
                                auxiliar = long.Parse(xFilialLog+"00000000"); 
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                    }
                    idHab = (auxiliar + 1).ToString();
                    return idHab;
                }
            }
        }

        public string UltimoNroIdRenglonesLim( string xModulo, string xBase, string xUserLog,string xFilialLog)
        {
            string sSQL = "";
            long auxiliar = 0;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSQL = sSQL + "SELECT MAX(ID) AS MAXIMO FROM RENGLONES WITH(NOLOCK) WHERE  ID >= " + xFilialLog + "00000001 AND ID <= " + xFilialLog + "99999999";

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
                            if ((dt.Rows[0]["MAXIMO"].ToString()) != null && (dt.Rows[0]["MAXIMO"].ToString()) != "")
                            {
                                string auxiliar1 = (dt.Rows[0]["MAXIMO"].ToString());
                                auxiliar = long.Parse(auxiliar1);
                            }
                            else
                            {
                                auxiliar = long.Parse(xFilialLog + "00000000");
                            }
                        }
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                    }
                }
            }
           
                string sSQL1 = "";
                long aux = 0;
                using (SqlConnection con1 = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
                {
                    sSQL1 = sSQL1 + "SELECT MAX(ID) AS MAXIMO FROM RENGLONESELIM WITH(NOLOCK) WHERE  ID >= " + xFilialLog + "00000001 AND ID <= " + xFilialLog + "99999999";

                    using (SqlCommand cmd1 = new SqlCommand(sSQL1, con1))
                    {
                        try
                        {
                            con1.Open();
                            DataTable dt1 = new DataTable();
                            SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                            da1.Fill(dt1);
                            if (dt1.Rows.Count > 0)
                            {
                                aux = long.Parse((dt1.Rows[0]["MAXIMO"].ToString()));
                            }
                            else
                            {
                                aux = 0;
                            }
                            con1.Close();
                        }
                        catch (Exception ex)
                        {
                            con1.Close();
                        }

                    }
            }

                if (auxiliar > aux)
                {
                    return (auxiliar + 1).ToString();
                }
                else
                {
                    return (aux + 1).ToString();
                }
        }
        public string AltaCabecera(string xNroAfiliado, string xName, string xOs, string xPlan, string xFildeb, string xCap,
          string xFdesde, string xFhasta, string xTipoexp, string xCodconc, string xModcab, string xAfiliacion, string[] xCantmed,
          string[] xCodmed, string[] xMedicamento, string[] responsable, string[] xCoddoc, string[] xRespdoc,
          string[] xDescdoc, string xFepm, string[] xEstdoc, string xObs, string xUser, string idSur,
          string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            int xRegMed = 1;
            int returnValue = 0;

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xNroAfiliado) == true
                || EsStrNuloBlanco(xFdesde) == true
                || EsStrNuloBlanco(xFhasta) == true
                || EsStrNuloBlanco(xTipoexp) == true
                || EsStrNuloBlanco(xCodconc) == true
                || EsStrNuloBlanco(xPlan) == true
                || EsStrNuloBlanco(xCap) == true
                || EsStrNuloBlanco(xOs) == true
                || EsStrNuloBlanco(xFildeb) == true
                || (EsStrNuloBlanco(xName) == true && xName.Length <= 45))
            {
                xMensaje = "Existen campos obligatorios incompletos. Imposible continuar.";
                return xMensaje;
            }
            for (int i = 0; i < responsable.Length; i++){
                if (responsable[i].ToString() != "No Aplica" && responsable[i].ToString() != "Afiliado" && responsable[i].ToString() != "Prestador") {
                    return "El responsable no es correcto.";
                }
            }
            for (int i = 0; i < xRespdoc.Length; i++)
            {
                if (EsValAlfaNumerico(xRespdoc[i].ToString()) == false)
                {
                    xMensaje = "Existen campos obligatorios incompletos. Imposible continuar." ;
                    return xMensaje;
                }
            }
            for (int i = 0; i < xDescdoc.Length; i++)
            {
                if (EsValAlfaNumerico(xDescdoc[i].ToString()) == false)
                {
                    xMensaje = "Existen campos obligatorios incompletos. Imposible continuar." ;
                    return xMensaje;
                }
            }
            for (int i = 0; i < xEstdoc.Length; i++)
            {
                if (EsValAlfaNumerico(xEstdoc[i].ToString()) == false)
                {
                    xMensaje = "Existen campos obligatorios incompletos. Imposible continuar." ;
                    return xMensaje;
                }
            }
            //3) Validacion del tipo de dato recibido...
            if (EsValNumerico(xNroAfiliado) == false && xNroAfiliado.Length > 11)
             
            {
                xMensaje =  "El Nro de Afiliado no debe tener mas de 11 digitos." ;
                return xMensaje;
            }

            if (
             EsFechaValida(xFdesde) == false
             ||EsFechaValida(xFhasta) == false)
            
            {
                xMensaje =  "Las fechas desde y hasta no son válidas." ;
                return xMensaje;
            }

            if (ExisteCodCon(xCodconc, xModulo, xBase, xUserLog, xFilialLog) == false)
            {
                xMensaje =  "El Codigo de Concepto no existe." ;
                return xMensaje;
            }

            if (EsValAlfaNumerico(xTipoexp) == false && xTipoexp.Length == 1)
            {
                xMensaje =  "El Codigo TipoExp debe tener longitud de 1 caracter." ;
                return xMensaje;
            }

            xMensaje = ExistePlan(xPlan, xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            xMensaje = ExisteFilDebCap(xFildeb, xCap, xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            xMensaje = ExisteOs(xOs, xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            xMensaje = ExisteModulo(xCodconc, xModcab, xFdesde, xFhasta, xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            xMensaje = ValidaPeriodoCabecera(xNroAfiliado, xCodconc, xFdesde, xFhasta, xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            string xMaxNro = UltimoNroIdCabeceras( xModulo, xBase, xUserLog,xFilialLog);
            string VMOD = "";

            if (xModcab == "" || xModcab == null ||  xModcab == "0" )
            {
                VMOD = "0";
            }
            else
            {
                VMOD = "0" + xModcab;
            }

            string VFECHAPM = "";
            if (xFepm != "")
            {
                VFECHAPM = FormatoFechaBase(xFepm, "yyyy/mm/dd", false);
            }
            else
            {
                VFECHAPM = "NULL";
            }
            string VFilial, VEst;
            if (xFilialLog == "61")
            {
                VFilial = "60";
                VEst = "FINALIZADO";
            }
            else
            {
                VFilial = xFilialLog;
                VEst = "EN GESTION";
            }
            string xFilafil, xnroAfil, xBeneafil;

            xFilafil = xNroAfiliado.Substring(0, 2);
            xnroAfil = xNroAfiliado.Substring(2, 7);
            xBeneafil = xNroAfiliado.Substring(9, 2);
            //---
            try { 
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
                    {
                        sSql = sSql + "INSERT INTO CABECERAS VALUES ('" + xMaxNro + "',";
                        sSql = sSql + "'" + xFilafil + "','" + xnroAfil + "',";
                        sSql = sSql + "'" + xBeneafil + "','" + xName + "',";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "'" + xFildeb + "',";
                        sSql = sSql + "'" + xOs + "',";
                        sSql = sSql + "'" + xTipoexp + "',";
                        sSql = sSql + "'" + xCodconc + "',";
                        sSql = sSql + "'" + FormatoFechaBase(xFdesde, "yyyy/mm/dd", false) + "',";
                        sSql = sSql + "'" + FormatoFechaBase(xFhasta, "yyyy/mm/dd", false) + "',";
                        sSql = sSql + "'" + VMOD + "',";
                        sSql = sSql + "'" + xCap + "',";
                        sSql = sSql + "'" + xObs + "',";
                        if (VFECHAPM == "NULL")
                        {
                            sSql = sSql + "null" + ",";
                        }
                        else
                        {
                            sSql = sSql + "'" + VFECHAPM + "',";
                        }
                        sSql = sSql + "'" + VFilial + "',";
                        sSql = sSql + "'" + xFilialLog + "',";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "'" + "EN GESTION" + "',";
                        sSql = sSql + "'" + VEst + "',";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "'" + "NO" + "',";
                        sSql = sSql + "'" + "NO" + "',";
                        sSql = sSql + "'" + xUserLog + "',";
                        DateTime date = DateTime.Now;
                        sSql = sSql + "'" + FormatoFechaBase(date.ToString(), "yyyy/mm/dd", false) + "',";
                        sSql = sSql + "'" + FormatoFechaBase(date.ToString(), "yyyy/mm/dd", false) + "',";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "'" + "NO" + "',";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "" + "null" + ",";
                        sSql = sSql + "'" + xPlan + "',";
                       
                        if (idSur == "" || idSur == null)
                        {
                            sSql = sSql + "" + "null" + ")";
                        }
                        else
                        {
                            try
                            {
                                if (idSur.Trim().Length != 13)
                                {
                                    return "El campo Nro Solicitud Sur debe ser de 13 digitos";
                                }

                                sSql = sSql + "" + Int64.Parse(idSur) + ")";
                            }
                            catch (Exception e)
                            {
                                return "El campo Nro Solicitud Sur incorrecto - " + e.Message;
                            }
                        }
                        con.Open();
                        SqlCommand command = new SqlCommand(sSql,con);
                        returnValue = command.ExecuteNonQuery();

                        if (returnValue > 0)
                        {
                            sSql = "UPDATE RENGLONES SET IDCAB ='" + xMaxNro + "',";
                            sSql = sSql + " MODIMPORT = CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END  ";
                            sSql = sSql + " WHERE FILAFIL2 = '" + xFilafil + "' AND NROAFIL2 = '" + xnroAfil + "' AND BENEFAFIL2 ='" + xBeneafil + "'";
                            sSql = sSql + " AND CODCONC   ='" + xCodconc + "' AND FEPREST <= '" + FormatoFechaBase(xFhasta, "yyyy/mm/dd", true) + "'";
                            sSql = sSql + " AND FEPREST >= '" + FormatoFechaBase(xFdesde, "yyyy/mm/dd", true) + "' AND IDCAB = 0";

                            returnValue = 0;
                            SqlCommand command2 = new SqlCommand(sSql, con);
                            returnValue = command2.ExecuteNonQuery();
                            string VResp = "";

                            // Insertando los medicamentos del Front
                            for (int i = 0; i < xCodmed.Length; i++)
                            {
                                sSql = "INSERT INTO CAB_MEDICAMENTOS VALUES ('" + xMaxNro + "',";
                                sSql = sSql + "" + xRegMed + ",'" + xCantmed[i].ToString() + "',";
                                sSql = sSql + "'" + xCodmed[i].ToString() + "','')";

                                returnValue = 0;
                                SqlCommand command3 = new SqlCommand(sSql, con);
                                returnValue = command3.ExecuteNonQuery();

                                xRegMed = xRegMed + 1;
                            }
                            //Insertando los documentos
                            for (int i = 0; i < responsable.Length; i++)
                            {
                                switch (responsable[i].ToString())
                                {
                                    case "No Aplica":
                                        VResp = "N";
                                        break;
                                    case "Afiliado":
                                        VResp = "A";
                                        break;
                                    case "Prestador":
                                        VResp = "P";
                                        break;
                                }

                                if (xEstdoc[i].ToString() == "N")
                                {
                                    sSql = "INSERT INTO CAB_DOCUMENTACION VALUES ('" + xMaxNro + "',";
                                    sSql = sSql + "'" + xCoddoc[i].ToString() + "','" + "N" + "',";
                                    sSql = sSql + "NULL,NULL,NULL,NULL,NULL,NULL,'NO','NO',";
                                    sSql = sSql + "'" + VResp + "')";

                                }
                                else if (xEstdoc[i].ToString() == "S")
                                {
                                    sSql = "INSERT INTO CAB_DOCUMENTACION VALUES ('" + xMaxNro + "',";
                                    sSql = sSql + "'" + xCoddoc[i].ToString() + "','" + "S" + "',";
                                    sSql = sSql + "'" + xUserLog + "',";

                                    sSql = sSql + "'" + FormatoFechaBase(date.ToString(), "yyyy/mm/dd", false) + "',";
                                    sSql = sSql + "'" + FormatoFechaBase(date.ToString(), "yyyy/mm/dd", false) + "',";

                                    sSql = sSql + "NULL,NULL,NULL,'NO','NO',";
                                    sSql = sSql + "'" + VResp + "')";
                                }

                                returnValue = 0;
                                SqlCommand command4 = new SqlCommand(sSql, con);
                                returnValue = command4.ExecuteNonQuery();
                            }
                        }
                   }
                   scope.Complete();
               }
            }
            catch (TransactionAbortedException ex)
            {
                xMensaje = ex.Message;
            }
            catch (ApplicationException ex)
            {
                xMensaje = ex.Message;
            }

            return xMensaje;
        }

        string MenuABM.PrestadorConsumo(string xCodpres, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string PresCon = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (EsStrNuloBlanco(xCodpres) == true || EsValNumerico(xCodpres) == false)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "El código de prestador esta vacio. Imposible continuar." + '"' + "}]";
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT NOMPRESTADOR, CASE WHEN APELLIDO IS NOT NULL AND NOMBRE IS NOT NULL ";
                sSql = sSql + " THEN NOMBRE + ' ' + APELLIDO WHEN APELLIDO IS     NULL AND NOMBRE IS NOT NULL ";
                sSql = sSql + " THEN NOMBRE ELSE APELLIDO END AS NOMPRESTADOR2 ";
                sSql = sSql + " FROM GRAL_PRESTADORES WHERE   CODPRESTADOR ='" + xCodpres + "'";

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
                            PresCon = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                PresCon = PresCon + "{";
                                PresCon = PresCon + '"' + "NOMBRE" + '"' + ":" + '"' + dt.Rows[i]["NOMPRESTADOR"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    PresCon = PresCon + "},";
                                }
                                else
                                {
                                    PresCon = PresCon + "}";
                                };
                            }
                            PresCon = PresCon + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "El prestador NO Existe." + '"' + "}]";
                            objStatus.Status = "PRESTADORCONSUMO:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            PresCon = xMensaje;

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        PresCon = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            PresCon = PresCon + "{";
                            PresCon = PresCon + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                PresCon = PresCon + "}";
                            }
                            else
                            {
                                PresCon = PresCon + "},";
                            };
                        }
                        PresCon = PresCon + "]";
                    }
                    con.Close();
                    return PresCon;

                }
            }

        }


        string MenuABM.altaPrestadorCON(string xCodpres, string xNombre, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            bool AgregarHabNueva = false;
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            string ID;

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            // Validaciones...
            if (
                xNombre == null || xNombre == ""
                || EsValAlfaNumerico(xNombre) == false || xNombre.Length > 45
                || xCodpres == null || xCodpres == ""
               )
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su petición no puede realizarse." + '"' + "}]";
                return xMensaje;
            }

            //Validacion de Cuit Repetido
            WS2 objFuncionesGrales = new WS2();
            bool bMotivos = objFuncionesGrales.consultaPRESTADOR(xCodpres,  xModulo,  xBase,  xUserLog,  xFilialLog);
            if (bMotivos == true)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "El prestador ya existe." + '"' + "}]";
                return xMensaje;
            }

            AgregarHabNueva = false;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = " INSERT INTO GRAL_PRESTADORES VALUES ('" + xCodpres + "','" + xNombre + "',NULL,NULL,NULL,NULL,NULL,NULL,NULL,'NO','NO')";

                // Start a local transaction.
                con.Open();
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("AltaPrestadorTransaction");
                try
                {

                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    ID = "[{" + '"' + "resultadoAltaPresta" + '"' + ":" + '"' + "El prestador se dio de alta" + '"' + "}]";

                }
                catch (Exception ex)
                {
                    ID = "[{" + '"' + "resultadoAltaPresta" + '"' + ":" + '"' + "El prestador NO se dio de alta" + '"' + "}]";
                    // Attempt to roll back the transaction.
                    transaction.Rollback("AltaPrestadorTransaction");

                }
                con.Close();
                return ID;
            }
        }


        string MenuABM.CbtEgresoConsumos(string xCBTEEGRESO, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            var CbtEgr = new StringBuilder();

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje; //6000 08994527
            }
            if (EsStrNuloBlanco(xCBTEEGRESO) == true)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "El código de comprobante de ingreso esta vacio. Imposible continuar." + '"' + "}]";
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT NROCUENTA, NROCHEQUE, FECBTEEGRESO,BANCO ";
                sSql = sSql + " FROM  RENGLONES  WITH(NOLOCK)  ";
                sSql = sSql + " WHERE NROCBTEEGRESO ='" + xCBTEEGRESO + "'";


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
                            CbtEgr.Append("[");
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                CbtEgr.Append("{");
                                CbtEgr.Append('"' + "Cuenta" + '"' + ":" + '"' + dt.Rows[i]["NROCUENTA"].ToString() + '"' + ",");
                                CbtEgr.Append('"' + "Cheque" + '"' + ":" + '"' + dt.Rows[i]["NROCHEQUE"].ToString() + '"' + ",");
                                CbtEgr.Append('"' + "FeEgreso" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FECBTEEGRESO"].ToString(), "dd/mm/yyyy", false) + '"' + ",");
                                CbtEgr.Append('"' + "Banco" + '"' + ":" + '"' + dt.Rows[i]["BANCO"].ToString() + '"');

                                if (i < Contador - 1)
                                {
                                    CbtEgr.Append("},");
                                }
                                else
                                {
                                    CbtEgr.Append("}");
                                };
                            }
                            CbtEgr.Append("]");

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            xMensaje = "[]";
                            objStatus.Status = "COMPROBANTE DE INGRESO CONTABLE:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            CbtEgr = new StringBuilder();
                            CbtEgr.Append("[]");

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        CbtEgr.Append("[");
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            CbtEgr.Append("{");
                            CbtEgr.Append('"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"');
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                CbtEgr.Append("}");
                            }
                            else
                            {
                                CbtEgr.Append("},");
                            };
                        }
                        CbtEgr.Append("]");
                    }
                    con.Close();
                    return CbtEgr.ToString();

                }
            }
        }


        string MenuABM.BuscarBancoComsumo(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();

            string BancoCon = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT * FROM GRAL_BANCOS WITH(NOLOCK) ";
                sSql = sSql + " WHERE FILIAL= " + xFilialLog;




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
                            BancoCon = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                BancoCon = BancoCon + "{";
                                BancoCon = BancoCon + '"' + "NroBanco" + '"' + ":" + '"' + dt.Rows[i]["ID"].ToString() + '"' + ",";
                                BancoCon = BancoCon + '"' + "NOMBRE" + '"' + ":" + '"' + dt.Rows[i]["NOMBRE"].ToString() + '"' + ",";
                                BancoCon = BancoCon + '"' + "Cuenta" + '"' + ":" + '"' + dt.Rows[i]["Cuenta"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    BancoCon = BancoCon + "},";
                                }
                                else
                                {
                                    BancoCon = BancoCon + "}";
                                };
                            }
                            BancoCon = BancoCon + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "El Banco NO Existe." + '"' + "}]";
                            objStatus.Status = "Banco consumos:NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            BancoCon = xMensaje;

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        BancoCon = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            BancoCon = BancoCon + "{";
                            BancoCon = BancoCon + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                BancoCon = BancoCon + "}";
                            }
                            else
                            {
                                BancoCon = BancoCon + "},";
                            };
                        }
                        BancoCon = BancoCon + "]";
                    }
                    con.Close();
                    return BancoCon;

                }
            }
        }

        string MenuABM.AltaConsumoGeneral(string xNroAfiliado, string xNombreAfil, string xNroFAC, string xFechaFact, string xImporteFAC,
            string xImpoorteFacPres, string xNotaCred, string xCodPrestador, string xnombrePrest, string xNroRemito, string xFechaRemito,
            string xConcepto, string xFechaPrestacion, string xOrdPractica, string xTramite, string xFechaCierre, string xCbteEgreso,
            string xFechaPago, string xNroCheque, string xCodBanco, string xNombrebanco, string xNroCuenta, string xNroRecibo,
            string xFechaRecibo, string xFechaDebito, string[] xIDAnticipo, string[] xAnticipos, string[] xNroReg, string[] xCantmed,
            string[] xCodMedic, string[] xPrecioUN, string xSaldoFC, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            bool xSaltoAlSiguiente = false;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string AltaCons = "";
            string xFilafil = xNroAfiliado.Substring(0, 2);
            string xnroAfil = xNroAfiliado.Substring(2, 7);
            string xBeneafil = xNroAfiliado.Substring(9, 2);
            string ID = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xNroAfiliado) == true
                || EsStrNuloBlanco(xNroFAC) == true
                || EsStrNuloBlanco(xFechaFact) == true
                || EsStrNuloBlanco(xImporteFAC) == true
                || EsStrNuloBlanco(xImpoorteFacPres) == true
                || EsStrNuloBlanco(xCodPrestador) == true
                || EsStrNuloBlanco(xNroRemito) == true
                || EsStrNuloBlanco(xFechaRemito) == true
                || EsStrNuloBlanco(xConcepto) == true
                || EsStrNuloBlanco(xFechaPrestacion) == true
                || EsStrNuloBlanco(xCbteEgreso) == true
                || EsStrNuloBlanco(xFechaPago) == true
                || EsStrNuloBlanco(xNroCheque) == true
                || EsStrNuloBlanco(xCodBanco) == true
                || EsStrNuloBlanco(xNombrebanco) == true
                || EsStrNuloBlanco(xNroCuenta) == true
            )
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su petición no puede realizarse." + '"' + "}]";
                return xMensaje;
            }

            //Formato de valores
            xFechaRemito = FormatoFechaBase(xFechaRemito, "yyyy/mm/dd", true);
            xFechaFact = FormatoFechaBase(xFechaFact, "yyyy/mm/dd", true);
            xFechaPrestacion = FormatoFechaBase(xFechaPrestacion, "yyyy/mm/dd", true);
            xFechaCierre = FormatoFechaBase(xFechaCierre, "yyyy/mm/dd", true);
            xFechaPago = FormatoFechaBase(xFechaPago, "yyyy/mm/dd", true);
            xFechaRecibo = FormatoFechaBase(xFechaRecibo, "yyyy/mm/dd", true);
            xFechaDebito = FormatoFechaBase(xFechaDebito, "yyyy/mm/dd", true);
            //-----------------------------------------------------------
            // Tomo el maximo ID
            // a partir de una comparacion de renglones y rengloneselim

            string xMaxNroLim = UltimoNroIdRenglonesLim(xModulo, xBase, xUserLog, xFilialLog);

            ID = xMaxNroLim;

            //-----------------------------------------------------------
            // Recuperacion de valores Adicionales
            string xIDCAB = "";
            string xUBICADM = "";
            string xESTADOADM = "";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT ID,UBICADM, ESTADOADM FROM CABECERAS WITH(NOLOCK)   ";
                sSql = sSql + " WHERE FILAFIL = '" + xFilafil + "' AND NROAFIL = '" + xnroAfil + "'  AND BENEFAFIL = '" + xBeneafil + "'";
                sSql = sSql + " AND CODCONC   = '" + xConcepto + "'  ";
                sSql = sSql + " AND ESTADOCBLE ='EN GESTION' and FEDESDE <= '" + xFechaPrestacion + "' and FEHASTA >= '" + xFechaPrestacion + "'";

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();

                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(dt);

                    int Contador = dt.Rows.Count;
                    if (dt.Rows.Count > 0)
                    {
                        for (int ii = 0; ii < dt.Rows.Count; ii++)
                        {
                            xIDCAB = dt.Rows[ii]["ID"].ToString();
                            xUBICADM = dt.Rows[ii]["UBICADM"].ToString();
                            xESTADOADM = dt.Rows[ii]["ESTADOADM"].ToString();
                        }
                    }
                    else
                    {
                        xIDCAB = "0";
                        xUBICADM = "";
                        xESTADOADM = "";

                    }
                    con.Close();
                }
            }
                xNroFAC = FormatoFac(xNroFAC);
                xNroRemito = FormatoRemito(xNroRemito);

                if (xNotaCred == "-1")
                {
                    xImporteFAC = "-" + xImporteFAC;
                    xImpoorteFacPres = "-" + xImpoorteFacPres;
                }

                // 1) INSERT EN RENGLONES...
                sSql = "INSERT INTO RENGLONES VALUES (";
                sSql = sSql + "'" + ID + "',";
                sSql = sSql + "'" + xIDCAB + "',";
                sSql = sSql + "'" + xFilafil + "',";
                sSql = sSql + "'" + xnroAfil + "',";
                sSql = sSql + "'" + xBeneafil + "',";
                sSql = sSql + "'" + xFilafil + "',";
                sSql = sSql + "'" + xnroAfil + "',";
                sSql = sSql + "'" + xBeneafil + "',";
                sSql = sSql + "'" + xConcepto + "',";
                if (xOrdPractica == "" || xOrdPractica == null)
                {
                    sSql = sSql + "NULL,";
                }
                else
                {
                    sSql = sSql + "'" + xOrdPractica + "',";
                }
                sSql = sSql + "'" + xImporteFAC + "',";
                sSql = sSql + "'" + xImpoorteFacPres + "',";
                sSql = sSql + "'" + xFechaPrestacion + "',";
                if (xFechaCierre == "" || xFechaCierre == null || xFechaCierre == "1900-01-01")
                {
                    sSql = sSql + "NULL,";
                }
                else
                {
                    sSql = sSql + "'" + xFechaCierre + "',";
                }
                if (xTramite == "" || xTramite == null)
                {
                    sSql = sSql + "NULL,";
                }
                else
                {
                    sSql = sSql + "'" + xTramite + "',";
                }
                if (xCodPrestador == "" || xCodPrestador == null)
                {
                    sSql = sSql + "NULL,";
                }
                else
                {
                    sSql = sSql + "'" + xCodPrestador + "',";
                }

                sSql = sSql + "'" + xnombrePrest + "',";
                sSql = sSql + "'" + xNroFAC + "',";
                sSql = sSql + "'" + xFechaFact + "',";
                sSql = sSql + "'" + xCbteEgreso + "',";
                sSql = sSql + "'" + xFechaPago + "',";
                sSql = sSql + "'" + xNroCheque + "',";
                sSql = sSql + "'" + xNombrebanco + "',";
                sSql = sSql + "'" + xNroCuenta + "',";
                if (xNroRecibo == "" || xNroRecibo == null)
                {
                    sSql = sSql + "NULL,";
                }
                else
                {
                    sSql = sSql + "'" + xNroRecibo + "',";
                }
                if (xFechaRecibo == "" || xFechaRecibo == null || xFechaRecibo == "1900-01-01")
                {
                    sSql = sSql + "NULL,";
                }
                else
                {
                    sSql = sSql + "'" + xFechaRecibo + "',";
                }
                if (xFechaDebito == "" || xFechaDebito == null || xFechaDebito == "1900-01-01")
                {
                    sSql = sSql + "NULL,";
                }
                else
                {
                    sSql = sSql + "'" + xFechaDebito + "',";
                }
                sSql = sSql + "NULL,";
                sSql = sSql + "'" + xNroRemito + "',";
                sSql = sSql + "'" + xFechaRemito + "',";
                sSql = sSql + "'" + xUserLog + "',";
                DateTime date = DateTime.Now;
                sSql = sSql + "'" + FormatoFechaBase(date.ToString(), "yyyy/mm/dd", false) + "',";
                sSql = sSql + "'" + FormatoFechaBase(date.ToString(), "yyyy/mm/dd", false) + "',";
                sSql = sSql + "NULL,NULL,NULL,NULL,NULL,'NO','NO','" + xFilialLog + "',NULL,NULL,NULL,NULL)";

                int returnValue = 0;

                try
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
                        {
                            con.Open();
                            SqlCommand command = new SqlCommand(sSql, con);
                            returnValue = command.ExecuteNonQuery();

                            if (returnValue > 0)
                            {
                                try
                                {
                                    for (int j = 0; j < xIDAnticipo.Length; j++)
                                    {
                                        sSql = "INSERT INTO RENG_RECIBOSANTICIPOS VALUES ('" + ID + "',";
                                        sSql = sSql + "'" + xIDAnticipo[j].ToString() + "',";
                                        sSql = sSql + "'" + xAnticipos[j].ToString() + "',";
                                        sSql = sSql + "NULL,'NO','NO')";
                                        
                                        returnValue = 0;
                                        SqlCommand command2 = new SqlCommand(sSql, con);
                                        returnValue = command2.ExecuteNonQuery();

                                        if (returnValue > 0)
                                        {
                                            xSaltoAlSiguiente = true;
                                        }
                                        else
                                        {
                                            xSaltoAlSiguiente = false;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    AltaCons = "[{" + '"' + "resultadoAltaConsumo" + '"' + ":" + '"' + "0" + '"' + "}]";
                                    con.Close();
                                    return AltaCons;
                                }

                                if (xSaltoAlSiguiente == true)
                                {
                                    try
                                    {
                                        for (int k = 0; k < xNroReg.Length; k++)
                                        {
                                            sSql = " INSERT INTO RENG_DESCRIPCION VALUES ('" + ID + "',";
                                            sSql = sSql + "'" + xNroReg[k].ToString() + "','" + xCantmed[k].ToString() + "',";
                                            sSql = sSql + "'" + xCodMedic[k].ToString() + "','','','" + xPrecioUN[k].ToString() + "',";
                                            sSql = sSql + "'" + xUserLog + "',";
                                            DateTime date1 = DateTime.Now;
                                            sSql = sSql + "'" + FormatoFechaBase(date1.ToString(), "yyyy/mm/dd", false) + "',";
                                            sSql = sSql + "'" + FormatoFechaBase(date1.ToString(), "yyyy/mm/dd", false) + "',";
                                            sSql = sSql + "NULL,'NO','NO',NULL)";

                                            returnValue = 0;
                                            SqlCommand command3 = new SqlCommand(sSql, con);
                                            returnValue = command3.ExecuteNonQuery();
                                        }

                                        AltaCons = "[{" + '"' + "resultadoAltaConsumo" + '"' + ":" + '"' + "1" + '"' + "}]";

                                        if (xUBICADM != xFilialLog && xIDCAB != "0")
                                        {
                                            AltaCons = "[{" + '"' + "resultadoAltaConsumo" + '"' + ":" + '"' + "Atención: La ubicación Administrativa es diferente a la del consumo. Ubic Adm:" + xUBICADM + " Estado Adm.:" + xESTADOADM + '"' + "}]";
                                        }
                                        else
                                        {
                                            AltaCons = "[{" + '"' + "resultadoAltaConsumo" + '"' + ":" + '"' + "El Alta del consumo se realizo correctamente." + '"' + "}]";
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        AltaCons = "[{" + '"' + "resultadoAltaConsumo" + '"' + ":" + '"' + "0" + '"' + "}]";
                                    }
                                }
                                else
                                {
                                    AltaCons = "[{" + '"' + "resultadoAltaConsumo" + '"' + ":" + '"' + "0" + '"' + "}]";
                                }
                            }
                        }
                    }
                }
                catch (TransactionAbortedException ex)
                {
                    xMensaje = ex.Message;
                }
                catch (ApplicationException ex)
                {
                    xMensaje = ex.Message;
                }

                return AltaCons;
        }

        string MenuABM.EliminarConsumoDiscapacidad(string xIDreg, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();


            string Del_Doc_Disca = "";
            // Validacion...
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            //Comienzo de la operacion de borrado...
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "DELETE FROM RENGLONES WHERE ID = '" + xIDreg + "'";

                con.Open();

                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("deldocdisca");
                try
                {
                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    // Si el primer delete salio bien, procedo con el sengundo...
                    if (command.ExecuteNonQuery() > 0)
                    {
                        try
                        {
                            sSql = "DELETE FROM RENG_RECIBOSANTICIPOS WHERE idreng = '" + xIDreg + "'";
                            command.CommandText = sSql;
                            command.ExecuteNonQuery();
                            // Si el segundo delete salio bien, comiteo la transaccion...
                            transaction.Commit();
                            xMensaje = "El Documento fue Eliminado";
                        }
                        catch (Exception ex)
                        {
                            xMensaje = "Atencion:El Documento NO fue Eliminado";
                            // Algo salio mal, hago roll back de la transaccion.
                            transaction.Rollback("deldocdisca");
                        }
                    }
                    else
                    {
                        xMensaje = "Atencion:El Documento NO fue Eliminado";
                        // Algo salio mal, hago roll back de la transaccion.
                        transaction.Rollback("deldocdisca");
                    }
                    //-------------------------------------
                }
                catch (Exception ex)
                {
                    xMensaje = "Atencion:El Documento NO fue Eliminado";
                    // Algo salio mal, hago roll back de la transaccion.
                    transaction.Rollback("deldocdisca");

                }
                con.Close();
                Del_Doc_Disca = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + "}]";
                return Del_Doc_Disca;
            }

        }

        string MenuABM.AltaConsumoDiscapacidad(string xNroAfiliado, string xCodPrestador, string xnombrePrest,
            string xNroFAC, string xFechaFact, string xImporteFAC, string xImpoorteFacPres,
            string xNroRecibo, string xFechaRecibo, string xFechaPrestacion,
            string xTramite, string xReintCAP, string xReintNRO,
            string xCbteEgreso, string xFechaEgreso, string xNroCheque, string xCodBanco, string xNombrebanco,
            string xNroCuenta, string[] xIDAnticipo, string[] xAnticipo, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string AltaCons = "";
            string xFilafil = xNroAfiliado.Substring(0, 2);
            string xnroAfil = xNroAfiliado.Substring(2, 7);
            string xBeneafil = xNroAfiliado.Substring(9, 2);
            string ID = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);

            if (xMensaje != "")
            {
                return xMensaje;
            }

            if (EsStrNuloBlanco(xNroAfiliado) == true
                || EsStrNuloBlanco(xNroFAC) == true
                || EsStrNuloBlanco(xFechaFact) == true
                || EsStrNuloBlanco(xImporteFAC) == true
                || EsStrNuloBlanco(xImpoorteFacPres) == true
                || EsStrNuloBlanco(xCodPrestador) == true
                || EsStrNuloBlanco(xFechaPrestacion) == true
                || EsStrNuloBlanco(xCbteEgreso) == true
                || EsStrNuloBlanco(xNroCheque) == true
                || EsStrNuloBlanco(xCodBanco) == true
                || EsStrNuloBlanco(xNombrebanco) == true
                || EsStrNuloBlanco(xNroCuenta) == true
            )
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su petición no puede realizarse." + '"' + "}]";
                return xMensaje;
            }

            xFechaFact = FormatoFechaBase(xFechaFact, "yyyy/mm/dd", true);
            xFechaPrestacion = FormatoFechaBase(xFechaPrestacion, "yyyy/mm/dd", true);
            xFechaEgreso = FormatoFechaBase(xFechaEgreso, "yyyy/mm/dd", true);
            xFechaRecibo = FormatoFechaBase(xFechaRecibo, "yyyy/mm/dd", true);

            string xMaxNro = UltimoNroIdRenglones(xModulo, xBase, xUserLog, xFilialLog);
            ID = xMaxNro;

            string xIDCAB = "";
            string xUBICADM = "";
            string xESTADOADM = "";
            string xCODCONC = "";
            int returnValue = 0;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = " SELECT ID,CODCONC,UBICADM,ESTADOADM FROM CABECERAS  WITH(NOLOCK)  "; 
                sSql = sSql + " WHERE  (FILAFIL = '" + xFilafil + "' AND NROAFIL = '" + xnroAfil + "'  AND   BENEFAFIL = '" + xBeneafil + "'";   
                sSql = sSql + " and FEDESDE <= '" + xFechaPrestacion + "' and FEHASTA >= '" + xFechaPrestacion + "' AND CODCONC='DISCAPACIDAD'";
                sSql = sSql + " OR  FILAFIL = '" + xFilafil + "' AND NROAFIL = '" + xnroAfil + "'  AND BENEFAFIL = '" + xBeneafil + "'";  
                sSql = sSql + " and FEDESDE <= '" + xFechaPrestacion + "'";
                sSql = sSql + " and FEHASTA >= '" + xFechaPrestacion + "' AND CODCONC='DROGADEPENDENCIA') ";
                sSql = sSql + " AND CABECERAS.ESTADOCBLE='EN GESTION' ";
                sSql = sSql + " ORDER BY CODCONC";     

                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    DataTable dt = new DataTable();

                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(dt);

                    int Contador = dt.Rows.Count;
                    if (dt.Rows.Count > 0)
                    {
                        for (int ii = 0; ii < dt.Rows.Count; ii++)
                        {
                            xIDCAB = dt.Rows[ii]["ID"].ToString();
                            xCODCONC = dt.Rows[ii]["CODCONC"].ToString();
                            xUBICADM = dt.Rows[ii]["UBICADM"].ToString();
                            xESTADOADM = dt.Rows[ii]["ESTADOADM"].ToString();
                        }
                    }
                    else
                    {
                        xIDCAB = "0";
                        xCODCONC = "DISCAPACIDAD";
                        xUBICADM = "";
                        xESTADOADM = "";
                    }
                }
                con.Close();
             }

                xNroFAC = FormatoFac(xNroFAC);
                string xReintCAPNRO = FormatoCAPNRO(xReintCAP, xReintNRO);

                xMensaje = "El Alta del consumo se realizo correctamente.";

                try
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
                        {
                            sSql = "INSERT INTO RENGLONES VALUES (";
                            sSql = sSql + "'" + ID + "',";
                            sSql = sSql + "'" + xIDCAB + "',";
                            sSql = sSql + "'" + xFilafil + "',";
                            sSql = sSql + "'" + xnroAfil + "',";
                            sSql = sSql + "'" + xBeneafil + "',";
                            sSql = sSql + "'" + xFilafil + "',";
                            sSql = sSql + "'" + xnroAfil + "',";
                            sSql = sSql + "'" + xBeneafil + "',";
                            sSql = sSql + "'" + xCODCONC + "',";
                            sSql = sSql + "'" + xReintCAPNRO + "',";
                            sSql = sSql + "'" + xImporteFAC + "',";
                            sSql = sSql + "'" + xImpoorteFacPres + "',";
                            sSql = sSql + "'" + xFechaPrestacion + "',";
                            sSql = sSql + "NULL,";
                            if (xTramite == "" || xTramite == null)
                            {
                                sSql = sSql + "NULL,";
                            }
                            else
                            {
                                sSql = sSql + "'" + xTramite + "',";
                            }

                            if (xCodPrestador == "" || xCodPrestador == null)
                            {
                                sSql = sSql + "NULL,";
                            }
                            else
                            {
                                sSql = sSql + "'" + xCodPrestador + "',";
                            }

                            sSql = sSql + "'" + xnombrePrest + "',";
                            sSql = sSql + "'" + xNroFAC + "',";
                            sSql = sSql + "'" + xFechaFact + "',";
                            sSql = sSql + "'" + xCbteEgreso + "',";
                            sSql = sSql + "'" + xFechaEgreso + "',";
                            sSql = sSql + "'" + xNroCheque + "',";
                            sSql = sSql + "'" + xNombrebanco + "',";
                            sSql = sSql + "'" + xNroCuenta + "',";

                            if (xNroRecibo == "" || xNroRecibo == null)
                            {
                                sSql = sSql + "NULL,";
                            }
                            else
                            {
                                sSql = sSql + "'" + xNroRecibo + "',";
                            }

                            if (xFechaRecibo == "" || xFechaRecibo == null || xFechaRecibo == "1900-01-01")
                            {
                                sSql = sSql + "NULL,";
                            }
                            else
                            {
                                sSql = sSql + "'" + xFechaRecibo + "',";
                            }

                            if (xFechaEgreso == "" || xFechaEgreso == null || xFechaEgreso == "1900-01-01")
                            {
                                sSql = sSql + "NULL,";
                            }
                            else
                            {
                                sSql = sSql + "'" + xFechaEgreso + "',";
                            }

                            sSql = sSql + "NULL,";
                            sSql = sSql + "'" + "000000000000" + "',";
                            sSql = sSql + "'" + xFechaPrestacion + "',";
                            sSql = sSql + "'" + xUserLog + "',";
                            DateTime date = DateTime.Now;
                            sSql = sSql + "'" + FormatoFechaBase(date.ToString(), "yyyy/mm/dd", false) + "',";
                            sSql = sSql + "'" + FormatoFechaBase(date.ToString(), "yyyy/mm/dd", false) + "',";
                            sSql = sSql + "NULL,NULL,NULL,NULL,NULL,'NO','NO','" + xFilialLog + "',NULL,NULL,NULL,NULL)";

                            con.Open();
                            SqlCommand command = new SqlCommand(sSql, con);
                            returnValue = command.ExecuteNonQuery();

                            if (returnValue > 0)
                            {
                                for (int j = 0; j < xIDAnticipo.Length; j++)
                                {
                                    sSql = "INSERT INTO RENG_RECIBOSANTICIPOS VALUES ('" + ID + "',";
                                    sSql = sSql + "'" + xIDAnticipo[j].ToString() + "',";
                                    sSql = sSql + "'" + xAnticipo[j].ToString() + "',";
                                    sSql = sSql + "NULL,'NO','NO')";

                                    returnValue = 0;
                                    SqlCommand command1 = new SqlCommand(sSql, con);
                                    returnValue = command1.ExecuteNonQuery();

                                }

                                if (xUBICADM != xFilialLog && xIDCAB != "0")
                                {
                                    xMensaje = "Atención: La ubicación Administrativa es diferente a la del consumo. Ubic Adm:" + xUBICADM + " Estado Adm.:" + xESTADOADM;
                                }
                                else
                                {
                                    xMensaje = "El Alta del consumo se realizo correctamente.";
                                }

                                string auxReintCAPNRO = "";

                                if (xReintCAPNRO != "")
                                {
                                    auxReintCAPNRO = xReintCAPNRO;
                                }
                                else
                                {
                                    auxReintCAPNRO = xTramite;
                                }

                                AltaCons = "[{";
                                AltaCons = AltaCons + '"' + "Mensaje" + '"' + ":" + '"' + xMensaje + '"' + ",";
                                AltaCons = AltaCons + '"' + "ID" + '"' + ":" + '"' + ID + '"' + ",";
                                AltaCons = AltaCons + '"' + "NroAfil" + '"' + ":" + '"' + xNroAfiliado + '"' + ",";
                                AltaCons = AltaCons + '"' + "CUIT" + '"' + ":" + '"' + xCodPrestador + '"' + ",";
                                AltaCons = AltaCons + '"' + "Factura" + '"' + ":" + '"' + xNroFAC + '"' + ",";
                                AltaCons = AltaCons + '"' + "Importe" + '"' + ":" + '"' + xImporteFAC + '"' + ",";
                                AltaCons = AltaCons + '"' + "Reintegro" + '"' + ":" + '"' + auxReintCAPNRO + '"' + ",";
                                AltaCons = AltaCons + '"' + "Cbte" + '"' + ":" + '"' + xCbteEgreso + '"' + ",";
                                AltaCons = AltaCons + '"' + "FeCbte" + '"' + ":" + '"' + xFechaEgreso + '"' + ",";
                                AltaCons = AltaCons + '"' + "Cheque" + '"' + ":" + '"' + xNroCheque + '"' + ",";
                                AltaCons = AltaCons + '"' + "Banco" + '"' + ":" + '"' + xNombrebanco + '"' + ",";
                                AltaCons = AltaCons + '"' + "NroCuenta" + '"' + ":" + '"' + xNroCuenta + '"' + ",";
                                AltaCons = AltaCons + '"' + "FeFactura" + '"' + ":" + '"' + xFechaFact + '"' + ",";
                                AltaCons = AltaCons + '"' + "NroRecibo" + '"' + ":" + '"' + xNroRecibo + '"' + ",";
                                AltaCons = AltaCons + '"' + "FeRecibo" + '"' + ":" + '"' + xFechaRecibo + '"' + ",";
                                AltaCons = AltaCons + '"' + "FePrestacion" + '"' + ":" + '"' + xFechaPrestacion + '"';
                                AltaCons = AltaCons + "}]";
                            }
                            con.Close();
                        }
                        scope.Complete();
                    }
                }
                catch (TransactionAbortedException ex)
                {
                    xMensaje = ex.Message;
                }
                catch (ApplicationException ex)
                {
                    xMensaje = ex.Message;
                }

                return AltaCons;
            }

        string MenuABM.VerNotificaciones(string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string ID = "";
            var jNotif = new StringBuilder();
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);

            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "SELECT  CAB_NOTIFICACIONES.ID as ID, CAB_NOTIFICACIONES.NROEXPDTE as NROEXPDTE, CABECERAS.CAJA as CAJA, ";
                sSql = sSql + " CAB_NOTIFICACIONES.NRONOTIF AS NRONOTIF, CAB_NOTIFICACIONES.TIPONOTIF AS TIPONOTIF, ";
                sSql = sSql + " CAB_NOTIFICACIONES.INFONOTIF AS INFONOTIF, CAB_NOTIFICACIONES.FENOTIF AS FENOTIF, ";
                sSql = sSql + " CAB_NOTIFICACIONES.DIASVTO AS DIASVTO, CAB_NOTIFICACIONES.NROAVISO AS NROAVISO,";
                sSql = sSql + " CAB_NOTIFICACIONES.IDCAB AS IDCAB, CAB_NOTIFICACIONES.FECRE AS FECRE, CAB_NOTIFICACIONES.NROALTANOTA AS NROALTANOTA ";
                sSql = sSql + " FROM CAB_NOTIFICACIONES LEFT JOIN CABECERAS ON CAB_NOTIFICACIONES.IDCAB = CABECERAS.ID";
                sSql = sSql + " WHERE NROALTANOTA Is Null";

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
                            jNotif.Append("[");
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                //Validaciones
                                //1.	SI nroExpdte = Null entonces  [VnroExpdte]= '' sino  [VnroExpdte]= nroExpdte
                                string xNroExpediente = "";
                                if (dt.Rows[i]["NROEXPDTE"].ToString() == null || dt.Rows[i]["NROEXPDTE"].ToString() == "")
                                { xNroExpediente = ""; }
                                else
                                { xNroExpediente = dt.Rows[i]["NROEXPDTE"].ToString(); }

                                //2.	Si caja es distinto de Vacio entonces [Vcaja] = caja sino  [Vcaja] = "SIN CAB."
                                string xCaja = "";
                                if (dt.Rows[i]["caja"].ToString() == null || dt.Rows[i]["caja"].ToString() == "")
                                { xCaja = ""; }
                                else
                                { xCaja = dt.Rows[i]["caja"].ToString(); }

                                //3.	Si  nroNotif = Null entonces  [VnroNotif] = '' Sino  [VnroNotif] = nroNotif
                                string xnroNotif = "";
                                if (dt.Rows[i]["nroNotif"].ToString() == null || dt.Rows[i]["nroNotif"].ToString() == "")
                                { xnroNotif = ""; }
                                else
                                { xnroNotif = dt.Rows[i]["nroNotif"].ToString(); }

                                //4.	Si  INFONOTIF = Null  Entonces  [VInfoNotif] = '' Sino [VInfoNotif] = INFONOTIF
                                string xINFONOTIF = "";
                                if (dt.Rows[i]["INFONOTIF"].ToString() == null || dt.Rows[i]["INFONOTIF"].ToString() == "")
                                { xINFONOTIF = ""; }
                                else
                                { xINFONOTIF = dt.Rows[i]["INFONOTIF"].ToString(); }

                                //5.	Si DIASVTO = Null  Entonces  [VdiasVto] = '' Sino  [VdiasVto] = FENOTIF + DIASVTO (Formato DD/MM/YYYY)
                                string xDIASVTO = "";
                                if (dt.Rows[i]["DIASVTO"].ToString() == null || dt.Rows[i]["DIASVTO"].ToString() == "")
                                { xDIASVTO = ""; }
                                else
                                {
                                    DateTime xFecha;
                                    xFecha = DateTime.Parse(dt.Rows[i]["FENOTIF"].ToString());
                                    xFecha = xFecha.AddDays(Int32.Parse(dt.Rows[i]["DIASVTO"].ToString()));
                                    xDIASVTO = FormatoFecha(xFecha.ToString(), "dd/mm/yyyy", true);
                                }
                                //{ xDIASVTO = Fdt.Rows[i]["DIASVTO"].ToString() + FormatoFecha(dt.Rows[i]["FENOTIF"].ToString(), "dd/mm/yyyy", false); }

                                //6.	SI NROALTANOTA = Null  Entonces  [VNroAltaNota] = '' Sino  [VNroAltaNota] = NROALTANOTA
                                string xNROALTANOTA = "";
                                if (dt.Rows[i]["NROALTANOTA"].ToString() == null || dt.Rows[i]["NROALTANOTA"].ToString() == "")
                                { xNROALTANOTA = ""; }
                                else
                                { xNROALTANOTA = dt.Rows[i]["NROALTANOTA"].ToString(); }

                                jNotif.Append("{");
                                jNotif.Append('"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["ID"].ToString() + '"' + ",");
                                jNotif.Append('"' + "NroExpediente" + '"' + ":" + '"' + xNroExpediente + '"' + ",");
                                jNotif.Append('"' + "Caja" + '"' + ":" + '"' + xCaja + '"' + ",");
                                jNotif.Append('"' + "NroNotificacion" + '"' + ":" + '"' + xnroNotif + '"' + ",");
                                jNotif.Append('"' + "TipoNotif" + '"' + ":" + '"' + dt.Rows[i]["TipoNotif"].ToString() + '"' + ",");
                                jNotif.Append('"' + "Infosolic" + '"' + ":" + '"' + xINFONOTIF + '"' + ",");
                                jNotif.Append('"' + "FeRecep" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["fenotif"].ToString(), "dd/mm/yyyy", false) + '"' + ",");
                                jNotif.Append('"' + "FeVto" + '"' + ":" + '"' + FormatoFecha(xDIASVTO, "dd/mm/yyyy", true) + '"' + ",");
                                jNotif.Append('"' + "Nro" + '"' + ":" + '"' + dt.Rows[i]["NroAviso"].ToString() + '"' + ",");
                                jNotif.Append('"' + "AltaNota" + '"' + ":" + '"' + xNROALTANOTA + '"');

                                if (i < Contador - 1)
                                {
                                    jNotif.Append("},");
                                }
                                else
                                {
                                    jNotif.Append("}");
                                };
                            }
                            jNotif.Append("]");

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            jNotif.Append("[]");

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jNotif.Append("[");
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jNotif.Append("{");
                            jNotif.Append('"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"');
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jNotif.Append("}");
                            }
                            else
                            {
                                jNotif.Append("},");
                            };
                        }
                        jNotif.Append("]");
                    }
                    con.Close();
                    return jNotif.ToString();

                }
            }

        }
        //---------------------------------------------------------------------------------------------------------------
        string MenuABM.abm_rnos(string xFilial, string xnroAfil, string xBeneAfil, string[] xFedesde, string[] xFehasta, string[] xRnos, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            SqlTransaction transaction;
            string AltaRnos = "";

            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);

            if (xMensaje != "")
            {
                return xMensaje;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                con.Open();
                SqlCommand command = con.CreateCommand();
                transaction = con.BeginTransaction("RnosTransaction");
                command.Connection = con;
                command.Transaction = transaction;

                sSql = "DELETE FROM GRAL_AFILIADOS_SSS ";
                sSql = sSql + " WHERE filafil='" + xFilial + "'";
                sSql = sSql + " AND nroafil='" + xnroAfil + "' AND benefafil='" + xBeneAfil + "'";
                command.CommandText = sSql;
                command.ExecuteNonQuery();
    
                for (int i = 0; i < xFedesde.Length; i++)
                {
                    try
                    {
                        if (xFedesde[i].ToString() == null || xFedesde[i].ToString() == "") {
                            return "No se puede procesar su solicitud.";
                        } else { 
                            xFedesde[i] = FormatoFecha(xFedesde[i].ToString(), "yyyy/mm/dd", true);
                            xFehasta[i] = FormatoFecha(xFehasta[i].ToString(), "yyyy/mm/dd", true);
                        }

                        sSql = "INSERT INTO GRAL_AFILIADOS_SSS VALUES ('" + xFilial + "',";
                        sSql = sSql + "'" + xnroAfil + "','" + xBeneAfil + "',";
                        sSql = sSql + "'" + xFedesde[i].ToString() + "',";
                        if (xFehasta[i].ToString() == "")
                        { 
                            sSql = sSql + "" + "NULL" + ",";
                        }
                        else
                        { 
                            sSql = sSql + "'" + xFehasta[i].ToString() + "',"; 
                        }
                        sSql = sSql + "'" + xRnos[i].ToString() + "')";

                        command.CommandText = sSql;
                        command.ExecuteNonQuery();

                        AltaRnos = "[{" + '"' + "resultadoModiDisca" + '"' + ":" + '"' + "1" + '"' + "}]";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback("RnosTransaction");
                        AltaRnos = "[{" + '"' + "resultadoRnos" + '"' + ":" + '"' + "0" + '"' + "}]";
                        con.Close();
                        return AltaRnos;
                    }
                } 
 
                try
                {
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback("RnosTransaction");
                    AltaRnos = "[{" + '"' + "resultadoRnos" + '"' + ":" + '"' + "0" + '"' + "}]";
                }

                con.Close();
            }

            return AltaRnos;
        }
        public string ExpEnvioSedeCentral(string xFeDesde, string xFeHasta, string usuario, string modulo, string bd, string filialLogin)
        {

            string sSql = "";
            var jsonExpEnvioSedeCentral = new StringBuilder();
            try
            {
                string mensaje = ValidarDatosConexion(modulo, bd, usuario, filialLogin);
                if (mensaje != "")
                {
                    return mensaje;
                }

                if (EsFechaValida(xFeDesde) == false)
                {
                    return "Hay campos obligatorios que faltan completar.";
                }

                if (EsFechaValida(xFeHasta) == false)
                {
                    return "Hay campos obligatorios que faltan completar.";
                }

                if (CompararFechas(xFeDesde, xFeHasta) == false)
                {
                    return "La fecha Hasta no puede ser menor que la fecha Desde.";
                }
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[bd].ConnectionString))
                {
                    sSql = "SELECT ID, NOMBREAFIL, RIGHT('00' + cast(FILAFIL as varchar),2) + '-' + RIGHT('0000000' + cast(NROAFIL ";
                    sSql = sSql + " as varchar),7) + '-' + RIGHT('00' + cast(BENEFAFIL as varchar),2) AS NUMEROAFIL, ";
                    sSql = sSql + " CONVERT(VARCHAR(10),FEDESDE,103) + ' '  +  CONVERT(VARCHAR(10),FEHASTA,103) AS PERIODO, ";
                    sSql = sSql + " CODOS, GRAL_CONCEPTOS.PATOL_CODIF AS CODCONC, MOTIVOBAJAADM, CAB_SEGUIMIENTO.FECRE, ";
                    sSql = sSql + " CAB_SEGUIMIENTO.USCRE, ESTADONUE  FROM CABECERAS  WITH(NOLOCK) ";
                    sSql = sSql + " INNER JOIN CAB_SEGUIMIENTO  WITH(NOLOCK) ON CABECERAS.ID = CAB_sEGUIMIENTO.IDCAB ";
                    sSql = sSql + " JOIN GRAL_CONCEPTOS ON CABECERAS.CODCONC = GRAL_CONCEPTOS.CODCONC ";
                    sSql = sSql + " WHERE TIPO ='ADM'   ";

                    if (filialLogin.ToString() != "61")
                    {
                        sSql = sSql + " AND UBICANT = '" + filialLogin + "'";
                    }

                    sSql = sSql + " AND CAB_SEGUIMIENTO.FECRE >='" + FormatoFechaSinBarra(xFeDesde, "yyyymmdd", false) + "'";
                    sSql = sSql + " AND CAB_SEGUIMIENTO.FECRE <='" + FormatoFechaSinBarra(xFeHasta, "yyyymmdd", false) + "' AND UBICNUE = '61' ";
                    sSql = sSql + " ORDER BY ESTADONUE, CAB_SEGUIMIENTO.FECRE,ID";


                    using (SqlCommand cmd = new SqlCommand(sSql, con))
                    {
                        DataTable dt = new DataTable();
                        try
                        {
                            con.Open();
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.Fill(dt);
                            try
                            {
                                jsonExpEnvioSedeCentral.Append("[");
                                if (dt.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        jsonExpEnvioSedeCentral.Append ("{" + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["ID"].ToString() + '"' + ",");
                                        jsonExpEnvioSedeCentral.Append ( '"' + "NOMBREAFIL" + '"' + ":" + '"' + dt.Rows[i]["NOMBREAFIL"].ToString() + '"' + ",");
                                        jsonExpEnvioSedeCentral.Append ( '"' + "NUMEROAFIL" + '"' + ":" + '"' + dt.Rows[i]["NUMEROAFIL"].ToString() + '"' + ",");
                                        jsonExpEnvioSedeCentral.Append ( '"' + "PERIODO" + '"' + ":" + '"' + dt.Rows[i]["PERIODO"].ToString() + '"' + ",");
                                        jsonExpEnvioSedeCentral.Append ( '"' + "CODOS" + '"' + ":" + '"' + dt.Rows[i]["CODOS"].ToString() + '"' + ",");
                                        jsonExpEnvioSedeCentral.Append ( '"' + "CODCONC" + '"' + ":" + '"' + dt.Rows[i]["CODCONC"].ToString() + '"' + ",");
                                        jsonExpEnvioSedeCentral.Append ( '"' + "MOTIVOBAJAADM" + '"' + ":" + '"' + dt.Rows[i]["MOTIVOBAJAADM"].ToString() + '"' + ",");
                                        jsonExpEnvioSedeCentral.Append ('"' + "FECRE" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FECRE"].ToString(), "dd/mm/yyyy", false) + '"' + ",");
                                        jsonExpEnvioSedeCentral.Append ('"' + "USCRE" + '"' + ":" + '"' + dt.Rows[i]["USCRE"].ToString() + '"' + ",");
                                        jsonExpEnvioSedeCentral.Append ('"' + "ESTADONUE" + '"' + ":" + '"' + dt.Rows[i]["ESTADONUE"].ToString() + '"');
                                        if (i < (dt.Rows.Count - 1))
                                        {
                                            jsonExpEnvioSedeCentral.Append ( "},");
                                        }
                                        else
                                        {
                                            jsonExpEnvioSedeCentral.Append("}");
                                        };
                                    }
                                }
                                jsonExpEnvioSedeCentral.Append("]");
                            }
                            catch (Exception e)
                            {
                                return "Error Al armar el json con los datos obtenidos de la base: " + e.Message + ".";
                            }
                        }
                        catch (Exception e)
                        {
                            return "Error Base de datos: " + e.Message + ".";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return "Error Base de datos: " + e.Message + ".";
            }

            return jsonExpEnvioSedeCentral.ToString();
        }
    }
}
