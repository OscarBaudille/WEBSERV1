using System;
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
using System.IO;
using System.Transactions;

namespace WcfServiceAPE
{
    [ServiceBehavior(Namespace = "http://ar.com.osde/osgapeservice/", Name = "OSGAPEBackendService")]
    public class WS5 : MetodosToolbar
    {
        public string GxfilAfil;
        public string GxNroAfil;
        public string GxBenefAfil;
        
        public string buscarArchivos(string xIDCabecera, string xnroAfiliado, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string busqArch = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (xIDCabecera == null || xnroAfiliado == null)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su solicitud no es correcta." + '"' + "}]";
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT replace(RUTA,'\','\\\') AS RUTA,CODIGO,NOMBRE,NOMBREAFIL FROM GRAL_ARCHIVOS_ASOCIADOS WITH(NOLOCK)INNER JOIN CABECERAS WITH(NOLOCK)";
                sSql = sSql + "on GRAL_ARCHIVOS_ASOCIADOS.CODIGO = RIGHT('00' + cast(CABECERAS.FILAFIL as varchar),2) + '-' ";
                sSql = sSql + "+ RIGHT('0000000' + cast(CABECERAS.NROAFIL as varchar),7) + '-' + RIGHT('00' + CONVERT(VARCHAR(2),";
                sSql = sSql + "CABECERAS.BENEFAFIL),2) ";
                sSql = sSql + " WHERE CODIGO IN ('" + xnroAfiliado + "')";
                sSql = sSql + " GROUP BY RUTA,CODIGO,NOMBREAFIL,NOMBRE";
                sSql = sSql + " UNION ";
                sSql = sSql + "SELECT replace(RUTA,'\','\\\') AS RUTA,CODIGO,GRAL_ARCHIVOS_ASOCIADOS.NOMBRE,";
                sSql = sSql + "CASE WHEN RENGLONES.NOMPRESTADOR IS NULL then CASE WHEN GRAL_PRESTADORES.NOMBRE IS NULL THEN ";
                sSql = sSql + "CASE WHEN GRAL_PRESTADORES.APELLIDO IS NULL THEN '' ELSE GRAL_PRESTADORES.APELLIDO END ";
                sSql = sSql + "ELSE GRAL_PRESTADORES.NOMBRE + ' ' + GRAL_PRESTADORES.APELLIDO END else GRAL_PRESTADORES.NOMPRESTADOR ";
                sSql = sSql + "END AS NOMBREPREST FROM RENGLONES WITH(NOLOCK) ";
                sSql = sSql + "INNER JOIN (GRAL_PRESTADORES WITH(NOLOCK) ";
                sSql = sSql + "INNER JOIN GRAL_ARCHIVOS_ASOCIADOS WITH(NOLOCK) ";
                sSql = sSql + "ON  GRAL_PRESTADORES.CODPRESTADOR = GRAL_ARCHIVOS_ASOCIADOS.CODIGO ) ";
                sSql = sSql + "ON RENGLONES.CODPRESTADOR = GRAL_PRESTADORES.CODPRESTADOR  ";
                sSql = sSql + " WHERE IDCAB IN ('" + xIDCabecera + "')";
                sSql = sSql + " GROUP BY RUTA,GRAL_ARCHIVOS_ASOCIADOS.NOMBRE,CODIGO,GRAL_PRESTADORES.CODPRESTADOR,";
                sSql = sSql + "CASE WHEN RENGLONES.NOMPRESTADOR IS NULL then CASE WHEN GRAL_PRESTADORES.NOMBRE IS NULL ";
                sSql = sSql + "THEN CASE WHEN GRAL_PRESTADORES.APELLIDO IS NULL THEN '' ELSE GRAL_PRESTADORES.APELLIDO END ";
                sSql = sSql + "ELSE GRAL_PRESTADORES.NOMBRE + ' ' + GRAL_PRESTADORES.APELLIDO END else GRAL_PRESTADORES.NOMPRESTADOR END";

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
                            busqArch = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                busqArch = busqArch + "{";
                                busqArch = busqArch + '"' + "ruta" + '"' + ":" + '"' + dt.Rows[i]["RUTA"].ToString() + '"' + ",";
                                busqArch = busqArch + '"' + "numero" + '"' + ":" + '"' + dt.Rows[i]["CODIGO"].ToString() + '"' + ",";
                                busqArch = busqArch + '"' + "descripcion" + '"' + ":" + '"' + dt.Rows[i]["NOMBRE"].ToString() + '"' + ",";
                                busqArch = busqArch + '"' + "nombre" + '"' + ":" + '"' + dt.Rows[i]["NOMBREAFIL"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    busqArch = busqArch + "},";
                                }
                                else
                                {
                                    busqArch = busqArch + "}";
                                };
                            }
                            busqArch = busqArch + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            busqArch = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        busqArch = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            busqArch = busqArch + "{";
                            busqArch = busqArch + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                busqArch = busqArch + "}";
                            }
                            else
                            {
                                busqArch = busqArch + "},";
                            };
                        }
                        busqArch = busqArch + "]";
                    }
                    con.Close();
                    return busqArch;

                }
            }

        }

        public string cargarPrestadoresAnexos(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {

            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string busqArch = "";
            int xContaErr = 0;
            string xMensaje = "";
            SqlTransaction sqlTran = null;
            SqlDataAdapter ad = new SqlDataAdapter();
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (xIDCabecera == null)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su solicitud no es correcta." + '"' + "}]";
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT GRAL_PRESTADORES.CODPRESTADOR,GRAL_PRESTADORES.TIPO,CASE WHEN RENGLONES.NOMPRESTADOR IS NULL then ";
                sSql = sSql + " CASE WHEN NOMBRE IS NULL THEN CASE WHEN APELLIDO IS NULL THEN '' ELSE APELLIDO END ";
                sSql = sSql + " ELSE NOMBRE + ' ' + APELLIDO END else GRAL_PRESTADORES.NOMPRESTADOR END AS NOMBREPREST,TEL1 AS TEL,TEL2 AS MAIL,GRAL_PRESTADORES.CODESP FROM RENGLONES WITH(NOLOCK) ";
                sSql = sSql + " INNER JOIN GRAL_PRESTADORES WITH(NOLOCK) ON RENGLONES.CODPRESTADOR = GRAL_PRESTADORES.CODPRESTADOR  ";
                sSql = sSql + "  WHERE IDCAB IN ('" + xIDCabecera + "') ";
                sSql = sSql + "  GROUP BY GRAL_PRESTADORES.CODPRESTADOR,GRAL_PRESTADORES.TIPO,CASE WHEN RENGLONES.NOMPRESTADOR IS NULL then ";
                sSql = sSql + " CASE WHEN NOMBRE IS NULL THEN ";
                sSql = sSql + " CASE WHEN APELLIDO IS NULL THEN '' ELSE APELLIDO END ELSE NOMBRE + ' ' + APELLIDO END ";
                sSql = sSql + " else GRAL_PRESTADORES.NOMPRESTADOR END, TEL1, TEL2,GRAL_PRESTADORES.CODESP";


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
                            busqArch = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                busqArch = busqArch + "{";
                                busqArch = busqArch + '"' + "tipo" + '"' + ":" + '"' + dt.Rows[i]["tipo"].ToString() + '"' + ",";
                                busqArch = busqArch + '"' + "numero" + '"' + ":" + '"' + dt.Rows[i]["CODPRESTADOR"].ToString() + '"' + ",";
                                busqArch = busqArch + '"' + "descripcion" + '"' + ":" + '"' + dt.Rows[i]["NOMBREPREST"].ToString() + '"' + ",";
                                busqArch = busqArch + '"' + "telefono" + '"' + ":" + '"' + dt.Rows[i]["TEL"].ToString() + '"' + ",";
                                busqArch = busqArch + '"' + "mail" + '"' + ":" + '"' + dt.Rows[i]["Mail"].ToString() + '"' + ",";
                                busqArch = busqArch + '"' + "especialidad" + '"' + ":" + '"' + dt.Rows[i]["CODESP"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    busqArch = busqArch + "},";
                                }
                                else
                                {
                                    busqArch = busqArch + "}";
                                };
                            }
                            busqArch = busqArch + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            objStatus.Status = "NO EXISTEN RESGISTROS PARA EL CRITERIO DE BUSQUEDA";
                            busqArch = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        busqArch = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            busqArch = busqArch + "{";
                            busqArch = busqArch + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                busqArch = busqArch + "}";
                            }
                            else
                            {
                                busqArch = busqArch + "},";
                            };
                        }
                        busqArch = busqArch + "]";
                    }
                    con.Close();
                    return busqArch;

                }
            }

        }
        public string agregarArchivos(string xAfiliado, string xTipo, string xRuta, string xArchivo, string xComentario, string xUsuario, string xModulo, string xBase, string xUserLog, string xFilialLog)
        {
            StatusWs objStatus = new StatusWs();
            string sSql = "";
            string xMensaje = "";
            SqlDataAdapter ad = new SqlDataAdapter();
            string AgregarArchNuevo = "";
            xMensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
            if (xMensaje != "")
            {
                return xMensaje;
            }
            if (xAfiliado == null || xTipo == null || xRuta == null || xArchivo == null || xUsuario == null)
            {
                xMensaje = "[{" + '"' + "Mensaje" + '"' + ":" + '"' + "Su solicitud no es correcta.Solo puede venir vacío el Comentario." + '"' + "}]";
                return xMensaje;
            }
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {
                sSql = "INSERT INTO GRAL_ARCHIVOS_ASOCIADOS VALUES ('" + xAfiliado + "',";
                sSql = sSql + "'" + xTipo + "','" + xRuta + "',";
                sSql = sSql + "'" + xArchivo + "','" + xComentario + "',Null,";
                sSql = sSql + "'" + xUsuario + "','" + DateTime.Now.ToString("yyyy/MM/dd") + "',";
                sSql = sSql + "'" + DateTime.Now.ToString("HH:mm:ss") + "')";

                //TimeOfDay.ToString() + "')";
                //Comienzo la transacción...

                con.Open();
                SqlCommand command = con.CreateCommand();
                SqlTransaction transaction;

                transaction = con.BeginTransaction("AltaArchAsociados");

                try
                {

                    command.Connection = con;
                    command.Transaction = transaction;

                    command.CommandText = sSql;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    AgregarArchNuevo = "[{" + '"' + "MENSAJE" + '"' + ":" + '"' + "El documento fue dado de alta con éxito." + "" + '"' + "}]";

                }
                catch (Exception ex)
                {
                    transaction.Rollback("AltaArchAsociados");
                    AgregarArchNuevo = "[{" + '"' + "ERROR" + '"' + ":" + '"' + "El documento NO fue dado de alta con éxito." + "" + '"' + "}]";
                }

                con.Close();

            }
            return AgregarArchNuevo;

        }
      

        private string FormatoHora(string xTime, String xFormat, bool xFtime)
        {
            string xHora = "";
            string xMinuto = "";
            string xSegundo = "";

            if (xTime == null || xTime == "" || xTime == "01/01/1900 00:00:00" || xTime == "01-01-1900 00:00:00" || xTime == "1900-01-01 00:00:00.000")
            {
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
            }
            if (xFtime == true)
            {
                //Fecha en blanco o null, retorno como viene...
                xTime = "1900-01-01 " + xTime;
            }
            return xTime;
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

        public string envioSedeCentral(long[] idCabecera, string[] estadoAdm, string usuario, string modulo, string filialLogin, string bd)
        {
            string jsonSedeCentral="";

            try { 
                string mensaje = ValidarDatosConexion(modulo, bd, usuario, filialLogin);
                if (mensaje != "")
                {
                    return mensaje;
                } 

                if (idCabecera.Length < 1)
                {
                    return "ID Cabecera es un campo obligatorio.";
                }

                if (estadoAdm.Length < 1)
                {
                    return "Estado Administrativo es un campo obligatorio.";
                }

                if (idCabecera.Length != estadoAdm.Length)
                {
                    return "Los datos a procesar no son correlativos.";
                }
            }
            catch (Exception ex)
            {
                return "Error al procesar los datos de entrada, " + ex.Message +".";
            }

            int returnValue = 0;
            jsonSedeCentral = "[";
            for (int i = 0; i < idCabecera.Length; i++)
            {
                try
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[bd].ConnectionString))
                        {

                            string sql = "UPDATE CABECERAS SET ";
                            if (estadoAdm[i].ToString().ToUpper() == "EN GESTION")
                            {
                                sql = sql + "ESTADOADM ='FINALIZADO', ";
                            }
                            sql = sql + "MODIMPORTADM= CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END, ";
                            sql = sql + "UBICADM ='61', MODIMPORT = CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END ";
                            sql = sql + "WHERE ID = " + idCabecera[i];

                            con.Open();
                            SqlCommand command = new SqlCommand(sql, con);
                            returnValue = command.ExecuteNonQuery();

                            if (estadoAdm[i].ToString().ToUpper() == "EN GESTION")
                            {
                                sql = "INSERT INTO CAB_SEGUIMIENTO VALUES (" + idCabecera[i] + ",'" + estadoAdm[i].ToString() + "','FINALIZADO','ADM','" + usuario.ToString() + "','" + FormatoFecha2() + "','" + FormatoFecha2() + "','" + filialLogin.ToString() + "','61','NO')";

                                returnValue = 0;
                                SqlCommand command2 = new SqlCommand(sql, con);
                                returnValue = command2.ExecuteNonQuery();

                                if (i < (idCabecera.Length - 1))
                                {
                                    jsonSedeCentral = jsonSedeCentral + "{'EstadoAdm':'FINALIZADO','UbicacionAdm':'61'},";
                                }
                                else
                                {
                                    jsonSedeCentral = jsonSedeCentral + "{'EstadoAdm':'FINALIZADO','UbicacionAdm':'61'}";
                                }
                            }
                            else
                            {
                                if (estadoAdm[i].ToString().ToUpper() == "BAJA")
                                {
                                    sql = "INSERT INTO CAB_SEGUIMIENTO VALUES (" + idCabecera[i] + ",'" + estadoAdm[i].ToString() + "','ADM','" + usuario.ToString() + "','"+FormatoFecha2()+"','"+FormatoFecha2()+"','" + filialLogin.ToString() + "','61','NO')";

                                    returnValue = 0;
                                    SqlCommand command3 = new SqlCommand(sql, con);
                                    returnValue = command3.ExecuteNonQuery();
                                }
                                if (i < (idCabecera.Length - 1))
                                {
                                    jsonSedeCentral = jsonSedeCentral + "{'EstadoAdm':'" + estadoAdm[i].ToString() + "','UbicacionAdm':'61'},";
                                }
                                else
                                {
                                    jsonSedeCentral = jsonSedeCentral + "{'EstadoAdm':'" + estadoAdm[i].ToString() + "','UbicacionAdm':'61'}";
                                }
                            }
                        }
                        scope.Complete();
                    }
                }
                catch (TransactionAbortedException ex)
                {
                    return "Error Base de datos: " + ex.Message + ".";
                }
                catch (ApplicationException ex)
                {
                    return "Error Base de datos: " + ex.Message + ".";
                }
            }

            jsonSedeCentral = jsonSedeCentral + "]";

            return jsonSedeCentral;
        }
		
        public string buscarComentarios(long idCabecera, string usuario, string modulo, string filialLogin, string bd)
        {
            string sSql = "";
            string jsonbuscarComentarios = "";

            try
            {
                string mensaje = ValidarDatosConexion(modulo, bd, usuario, filialLogin);
                if (mensaje != "")
                {
                    return mensaje;
                }

                if (idCabecera < 0)
                {
                    return "ID Cabecera es un campo obligatorio.";
                }
            }
            catch (Exception ex)
            {
                return "Error al procesar los datos de entrada, " + ex.Message + ".";
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[bd].ConnectionString))
            {

                sSql = "SELECT * FROM CAB_COMENTARIOS WITH(NOLOCK) ";
                sSql = sSql + " WHERE IDCAB = " + idCabecera.ToString();
                sSql = sSql + " ORDER BY FECRE, HORACRE";

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
                            if (dt.Rows.Count > 0)
                            {
                                jsonbuscarComentarios = "[";
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    jsonbuscarComentarios = jsonbuscarComentarios + "{";
                                    jsonbuscarComentarios = jsonbuscarComentarios + '"' + "Usuario" + '"' + ":" + '"' + dt.Rows[i]["USCRE"].ToString() + '"' + ",";
                                    jsonbuscarComentarios = jsonbuscarComentarios + '"' + "FechaCreacion" + '"' + ":" + '"' + FormatoFecha(dt.Rows[i]["FECRE"].ToString(), "dd/mm/yyyy", false) + '"' + ",";
                                    jsonbuscarComentarios = jsonbuscarComentarios + '"' + "Comentario" + '"' + ":" + '"' + dt.Rows[i]["TEXTO"].ToString().Trim() + '"' + "}";

                                    if (i < (dt.Rows.Count - 1))
                                    {
                                        jsonbuscarComentarios = jsonbuscarComentarios + ",";
                                    }
                                }
                                jsonbuscarComentarios = jsonbuscarComentarios + "]";

                            }
                            else
                            {
                                jsonbuscarComentarios = "[]";
                            }
                        }
                        catch (Exception e)
                        {
                            return "Error al procesar datos obtenidos de la base: " + e.Message;
                        }
                    }
                    catch (SqlException ex)
                    {
                        return "Error al ejecutar la query: " + ex.Message;
                    }

                    con.Close();
                }
            }
            return jsonbuscarComentarios;
        }

        public string ingresarComent(long IdCabecera, string xComentario, string xUsuario, string usuario, string modulo, string filialLogin, string bd)
        {
            string sSql = "";
            SqlTransaction transaction;
            string jsoningresarComent = "";

            try
            {
                string mensaje = ValidarDatosConexion(modulo, bd, usuario, filialLogin);
                if (mensaje != "")
                {
                    return mensaje;
                }

                if (EsStrNuloBlanco(IdCabecera.ToString()) == true)
                {
                    return "ID Cabecera es un campo obligatorio.";
                }
            }
            catch (Exception ex)
            {
                return "Error al procesar los datos de entrada, " + ex.Message + ".";
            }
            jsoningresarComent = "[";
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[bd].ConnectionString))
                {
                    con.Open();
                    transaction = con.BeginTransaction("CommentTransaction");
                    try
                    {
                        SqlCommand command = con.CreateCommand();
                        command.Connection = con;
                        command.Transaction = transaction;

                        sSql = "INSERT INTO CAB_Comentarios ";
                        sSql = sSql + " VALUES (" + IdCabecera.ToString() + ",'" + xComentario + "','" + xUsuario + "','" + DateTime.Now.ToString("yyyy/MM/dd") + "','" + DateTime.Now.ToString("HH:mm:ss") + "')";
                        command.CommandText = sSql;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        con.Close();
                        return "Error Base de datos: " + ex.Message + ".";

                    }
                }
            }
            catch (Exception ex)
            {
                return "Error Base de datos: " + ex.Message + ".";

            }
            jsoningresarComent = jsoningresarComent + "Insert Realizado exitosamente.]";
            return jsoningresarComent;
        }
		
        public string verNotificacionesExp(long idCabecera, string usuario, string modulo, string filialLogin, string bd)
        {
            string jsonVerNotificacionesExp = "";

            try
            {
                string mensaje = ValidarDatosConexion(modulo, bd, usuario, filialLogin);
                if (mensaje != "")
                {
                    return mensaje;
                }

                if (idCabecera == null || idCabecera == 0)
                {
                    return "ID Cabecera es un campo obligatorio.";
                }
            }
            catch (Exception ex)
            {
                return "Error al procesar los datos de entrada, " + ex.Message + ".";
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[bd].ConnectionString))
                {
                    string sql = "SELECT CAB_NOTIFICACIONES.ID as ID, CAB_NOTIFICACIONES.NROEXPDTE as NROEXPDTE, CABECERAS.CAJA as CAJA, " +
                                 "CAB_NOTIFICACIONES.NRONOTIF AS NRONOTIF, CAB_NOTIFICACIONES.TIPONOTIF AS TIPONOTIF, " +
                                 "CAB_NOTIFICACIONES.INFONOTIF AS INFONOTIF, CAB_NOTIFICACIONES.FENOTIF AS FENOTIF, " +
                                 "CAB_NOTIFICACIONES.DIASVTO AS DIASVTO, CAB_NOTIFICACIONES.NROAVISO AS NROAVISO, " +
                                 "CAB_NOTIFICACIONES.IDCAB AS IDCAB, CAB_NOTIFICACIONES.FECRE AS FECRE, " +
                                 "CAB_NOTIFICACIONES.NROALTANOTA AS NROALTANOTA FROM CAB_NOTIFICACIONES " +
                                 "LEFT JOIN CABECERAS ON CAB_NOTIFICACIONES.IDCAB = CABECERAS.ID " +
                                 "WHERE IDCAB = " + idCabecera;

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        DataTable dt = new DataTable();
                        try
                        {
                            con.Open();
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.Fill(dt);
                            try{
                                jsonVerNotificacionesExp = "[";
                                if (dt.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        jsonVerNotificacionesExp = jsonVerNotificacionesExp + "{" + '"' + "ID" + '"' + ":" + '"' + dt.Rows[i]["ID"].ToString() + '"' + ",";
                                        if (dt.Rows[i]["NROEXPDTE"].ToString() == null || dt.Rows[i]["NROEXPDTE"].ToString() == "NULL" || dt.Rows[i]["NROEXPDTE"].ToString() == "")
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "NroExpediente" + '"' + ":" + '"' + " " + '"' + ",";
                                        }
                                        else
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "NroExpediente" + '"' + ":" + '"' + dt.Rows[i]["NROEXPDTE"].ToString() + '"' + ",";
                                        }
                                        if (dt.Rows[i]["CAJA"].ToString() == "" || dt.Rows[i]["CAJA"].ToString() == "NULL" || dt.Rows[i]["CAJA"].ToString() == null)
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "Caja" + '"' + ":" + '"' + "SIN CAB." + '"' + ",";                                            
                                        }
                                        else
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "Caja" + '"' + ":" + '"' + dt.Rows[i]["CAJA"].ToString() + '"' + ",";
                                        }
                                        if (dt.Rows[i]["NRONOTIF"].ToString() == null || dt.Rows[i]["NRONOTIF"].ToString() == "NULL" || dt.Rows[i]["NRONOTIF"].ToString() == "")
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "NroNotificacion" + '"' + ":" + '"' + " " + '"' + ",";
                                        }
                                        else
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "NroNotificacion" + '"' + ":" + '"' + dt.Rows[i]["NRONOTIF"].ToString() + '"' + ",";
                                        }
                                        jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "Tipo" + '"' + ":" + '"' + dt.Rows[i]["TIPONOTIF"].ToString() + '"' + ",";
                                        if (dt.Rows[i]["INFONOTIF"].ToString() == null || dt.Rows[i]["INFONOTIF"].ToString() == "NULL" || dt.Rows[i]["INFONOTIF"].ToString() == "")
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "InfoSolic" + '"' + ":" + '"' + " " + '"' + ",";   
                                        }
                                        else
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "InfoSolic" + '"' + ":" + '"' + dt.Rows[i]["INFONOTIF"].ToString() + '"' + ",";
                                        }
                                        jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "FechaRecep" + '"' + ":" + '"' + dt.Rows[i]["FENOTIF"].ToString() + '"' + ",";
                                        if (dt.Rows[i]["DIASVTO"].ToString() == null || dt.Rows[i]["DIASVTO"].ToString() == "NULL" || dt.Rows[i]["DIASVTO"].ToString() == "")
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "FechaVto" + '"' + ":" + '"' + " " + '"' + ",";  
                                        }
                                        else
                                        {
                                            DateTime fechaBase = DateTime.Parse(dt.Rows[i]["FENOTIF"].ToString());
                                            DateTime fechaBaseDiasVTO = fechaBase.AddDays(int.Parse(dt.Rows[i]["DIASVTO"].ToString()));
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "FechaVto" + '"' + ":" + '"' + fechaBaseDiasVTO + '"' + ",";
                                        }
                                        jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "Numero" + '"' + ":" + '"' + dt.Rows[i]["NROAVISO"].ToString() + '"' + ",";
                                        if (dt.Rows[i]["NROALTANOTA"].ToString() == null || dt.Rows[i]["NROALTANOTA"].ToString() == "NULL" || dt.Rows[i]["NROALTANOTA"].ToString() == "")
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "AltaNota" + '"' + ":" + '"' + " " + '"' + "}";
                                        }
                                        else
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + '"' + "AltaNota" + '"' + ":" + '"' + dt.Rows[i]["NROALTANOTA"].ToString() + '"' + "}";
                                        }
                                        if (i < (dt.Rows.Count - 1))
                                        {
                                            jsonVerNotificacionesExp = jsonVerNotificacionesExp + ",";
                                        }
                                    }
                                }
                                jsonVerNotificacionesExp = jsonVerNotificacionesExp + "]";
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
            
            return jsonVerNotificacionesExp;
        }
        //-----------------------------------------------------------------------------------------------------------
        public string Filialesactivas(string xIDCabecera, string xnroAfiliado, string modulo, string bd, string usuario, string filialLogin)
        {
            string sSql = "";
            string jsonFilialesactivas = "";
            int xContaErr = 0;

            try
            {
                string mensaje = ValidarDatosConexion(modulo, bd, usuario, filialLogin);
                if (mensaje != "")
                {
                    return mensaje;
                }

                if (EsStrNuloBlanco(xIDCabecera) == true)
                {
                    return "ID Cabecera es un campo obligatorio.";
                }

                //validar que el contenido de idCabecera sea numerico
                if (EsStrNuloBlanco(xnroAfiliado) == true)
                {
                    return "El nro. de Afiliado es un campo obligatorio.";
                }
            }
            catch (Exception ex)
            {
                return "Error al procesar los datos de entrada, " + ex.Message + ".";
            }
            jsonFilialesactivas = "[";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[bd].ConnectionString))
            {

                sSql = "SELECT * FROM GRAL_FILIALES WITH(NOLOCK)";
                sSql = sSql + " WHERE ESTFIL = 'A' AND  CODFIL <> '" + filialLogin + "'";
                sSql = sSql + " ORDER BY CODFIL";

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
                            jsonFilialesactivas = "[";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                //{["Filial":"[CODFIL]","Descripcion":"[DESCFIL]"]}
                                jsonFilialesactivas = jsonFilialesactivas + "{";
                                jsonFilialesactivas = jsonFilialesactivas + '"' + "Filial" + '"' + ":" + '"' + dt.Rows[i]["CODFIL"].ToString() + '"' + ",";
                                jsonFilialesactivas = jsonFilialesactivas + '"' + "Descripcion" + '"' + ":" + '"' + dt.Rows[i]["DESCFIL"].ToString() + '"';

                                if (i < Contador - 1)
                                {
                                    jsonFilialesactivas = jsonFilialesactivas + "},";
                                }
                                else
                                {
                                    jsonFilialesactivas = jsonFilialesactivas + "}";
                                };
                            }
                            jsonFilialesactivas = jsonFilialesactivas + "]";

                        }

                        else
                        {
                            // STATUS: NOTFOUND
                            jsonFilialesactivas = "[]";

                        }
                    }
                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jsonFilialesactivas = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jsonFilialesactivas = jsonFilialesactivas + "{";
                            jsonFilialesactivas = jsonFilialesactivas + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jsonFilialesactivas = jsonFilialesactivas + "}";
                            }
                            else
                            {
                                jsonFilialesactivas = jsonFilialesactivas + "},";
                            };
                        }
                        jsonFilialesactivas = jsonFilialesactivas + "]";
                    }

                    con.Close();
                }
            }
            return jsonFilialesactivas;
        }

        public string DerivarLegajo(string xIdcabecera, string xFderiva, string xUbcAdm, string xEstadoCble, string modulo, string bd, string usuario, string filialLogin)
        {
            string sSql = "";
            SqlTransaction transaction;
            string jsoningresarComent = "";

            try
            {
                string mensaje = ValidarDatosConexion(modulo, bd, usuario, filialLogin);
                if (mensaje != "")
                {
                    return mensaje;
                }

                if (EsStrNuloBlanco(xIdcabecera.ToString()) == true)
                {
                    return "ID Cabecera es un campo obligatorio.";
                }
                if (EsStrNuloBlanco(xFderiva.ToString()) == true ||
                    EsStrNuloBlanco(xUbcAdm.ToString()) == true ||
                    EsStrNuloBlanco(xEstadoCble.ToString()) == true)
                {
                    return "Los campos Fderiva, UbicAdmin y EstadoContable son obligatorios.";
                }


            }
            catch (Exception ex)
            {
                return "Error al procesar los datos de entrada, " + ex.Message + ".";
            }
            jsoningresarComent = "[";
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[bd].ConnectionString))
                {
                    con.Open();
                    transaction = con.BeginTransaction("DerivarLegajoTransaction");
                    try
                    {
                        SqlCommand command = con.CreateCommand();
                        command.Connection = con;
                        command.Transaction = transaction;

                        sSql = "UPDATE CABECERAS SET  UBICADM ='" + xFderiva + "', MODIMPORT = CASE IMPORTADO WHEN 'NO' then 'NO' else 'SI' END  WHERE ID = " + xIdcabecera.ToString();
                        command.CommandText = sSql;
                        command.ExecuteNonQuery();

                        sSql = "UPDATE RENGLONES SET UBIC= '" + xFderiva + "' WHERE IDCAB = " + xIdcabecera.ToString();
                        command.CommandText = sSql;
                        command.ExecuteNonQuery();

                        sSql = "INSERT INTO CAB_SEGUIMIENTO VALUES ";
                        sSql = sSql + " (" + xIdcabecera.ToString() + ",'" + xEstadoCble + "','" + xEstadoCble + "','ADM','" + usuario + "','" + DateTime.Now.ToString("yyyy/MM/dd") + "','" + DateTime.Now.ToString("HH:mm:ss") + "','" + xUbcAdm + "','" + xFderiva + "','NO')";
                        command.CommandText = sSql;
                        command.ExecuteNonQuery();

                        transaction.Commit();

                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        con.Close();
                        return "Error Base de datos: " + ex.Message + ".";

                    }
                }
            }
            catch (Exception ex)
            {
                return "Error Base de datos: " + ex.Message + ".";

            }
            jsoningresarComent = jsoningresarComent + "Los legajos fueron derivados a la filial:" + xFderiva;
            return jsoningresarComent;
        }


        public string FormatoFecha2()
        {
            DateTime date = DateTime.Now;

            int Year = date.Year;
            int Month = date.Month;
            int Day = date.Day;

            string anio = Year.ToString();
            string mes = Month.ToString();
            string dia = Day.ToString();

            if (Day < 10)
            {
                dia = "0" + dia;
            }

            if (Month < 10)
            {
                mes = "0" + mes;
            }

            string fecha1 = anio + "-" + mes + "-" + dia + " " + date.Hour + ":" + date.Minute + ":" + date.Second + ".000";

            return fecha1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
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
            }
            if (xTime == true)
            {
                xDate = xDate + " 00:00:00";
            }
            return xDate;
        }
        
    //--24/08/2018--------------------------------------------------------------------------------------------------------------
        //TONY LO TERMINA...
        public string ImprimirCaratula(long xIDCabecera, string xUserLog, string xModulo, string xFilialLog, string xBase)
        {
            try
            {
                string mensaje = ValidarDatosConexion(xModulo, xBase, xUserLog, xFilialLog);
                if (mensaje != "")
                {
                    return mensaje;
                }

                if (string.IsNullOrEmpty(xIDCabecera.ToString())||xIDCabecera==0)
                {
                    return "ID Cabecera es un campo obligatorio.";
                }
            }
            catch (Exception ex)
            {
                return "Error al procesar los datos de entrada, " + ex.Message + ".";
            }
            // Definicion de variables...
            // Tomo los dos primeros digitos del xIdCabecera
            int xContaErr = 0;
            string FOrigen = xIDCabecera.ToString().Substring(0, 2);
            string sSql = "";
            string jsonImprimirCaratura = "";
            string xfilAfil;
            string xnroAfil;
            string xBeneAfil;
            string xMensaje = "";
            string xConcepto = "";
            string xCap = "";
            string xFildebito = "";
            string xAfiliado = "";
            string xNombreAfil = "";
            string xFeDesde = "";
            string xFeHasta = "";
            string xObservaciones = "";
            string xUsuarioCab = "";
            string xPersonaSolicitada = "";
            string xEstadoAdm = "";
            string xFeCarga = "";
            string xFePedMed = "";
            string xTelefono = "";

            string xCapDesc = "";
            string xDescDoc = "";
            string xDescDoc2 = "";
            string xPatolCodif = "";
            string xDescDosis = "";
            string xDescCos = "";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[xBase].ConnectionString))
            {

                sSql = "SELECT * FROM CABECERAS WITH (NOLOCK) WHERE ID=" + xIDCabecera;
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
                            xNombreAfil = dt.Rows[0]["NOMBREAFIL"].ToString();
                            xfilAfil = dt.Rows[0]["FILAFIL"].ToString();
                            GxfilAfil = dt.Rows[0]["FILAFIL"].ToString();

                            xnroAfil = dt.Rows[0]["NROAFIL"].ToString();
                            GxNroAfil = xnroAfil;

                            xBeneAfil = dt.Rows[0]["BENEFAFIL"].ToString();
                            GxBenefAfil = xBeneAfil;

                            xAfiliado = xfilAfil.ToString().PadLeft(2, '0') + xnroAfil.ToString().PadLeft(7, '0') + xBeneAfil.ToString().PadLeft(2, '0');
                            xConcepto = dt.Rows[0]["CODCONC"].ToString();
                            xCap = dt.Rows[0]["CAP"].ToString();
                            xFildebito = dt.Rows[0]["FILDEBITO"].ToString();

                            xFeDesde = FormatoFecha(dt.Rows[0]["FEDESDE"].ToString(), "dd/mm/yyyy", false);
                            xFeHasta = FormatoFecha(dt.Rows[0]["FEHASTA"].ToString(), "dd/mm/yyyy", false);
                            xObservaciones = dt.Rows[0]["OBSERV"].ToString();

                            xFePedMed = FormatoFecha(dt.Rows[0]["FEPM"].ToString(), "dd/mm/yyyy", false);
                            xFeCarga = FormatoFecha(dt.Rows[0]["FECRE"].ToString(), "dd/mm/yyyy", false);
                            xEstadoAdm = dt.Rows[0]["ESTADOADM"].ToString();
                            xUsuarioCab = dt.Rows[0]["USCRE"].ToString();
                            xCap = dt.Rows[0]["CAP"].ToString();
                        }
                    }

                    catch (SqlException ex)
                    {
                        string xMesgError;
                        jsonImprimirCaratura = "[";
                        foreach (SqlError error in ex.Errors)
                        {
                            xMesgError = error.Message.Replace("\"", "'"); ;
                            jsonImprimirCaratura = jsonImprimirCaratura + "{";
                            jsonImprimirCaratura = jsonImprimirCaratura + '"' + "ERROR" + '"' + ":" + '"' + error.LineNumber + " - " + xMesgError + '"';
                            xContaErr = xContaErr + 1;
                            if (ex.Errors.Count == xContaErr)
                            {
                                jsonImprimirCaratura = jsonImprimirCaratura + "}";
                            }
                            else
                            {
                                jsonImprimirCaratura = jsonImprimirCaratura + "},";
                            };
                        }
                        jsonImprimirCaratura = jsonImprimirCaratura + "]";
                    }
                    //----------------------------------------------------------------------
                    if (string.IsNullOrEmpty(xCap) == false)
                    {
                        sSql = "SELECT * FROM GRAL_CAPS WITH(NOLOCK) WHERE CODCAP ='" + xCap + "' AND FILIAL='" + xFildebito + "'";
                        using (SqlCommand cmd1 = new SqlCommand(sSql, con))
                        {
                            DataTable dt1 = new DataTable();
                            try
                            {
                                //    con.Open();
                                SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                                da1.Fill(dt1);
                                int Contador = dt1.Rows.Count;
                                if (dt1.Rows.Count > 0)
                                {
                                    xCapDesc = dt1.Rows[0]["DESCCAP"].ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                return "Error al procesar los datos de entrada, " + ex.Message + ".";
                            }
                        }
                    }
                    //----------------------------------------------------------------------
                    sSql = "SELECT * FROM GRAL_AFILIADOS WITH(NOLOCK) WHERE FILAFIL=" + GxfilAfil + " AND NROAFIL=" + GxNroAfil + " AND BENEFAFIL=" + GxBenefAfil;
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

                                xTelefono = dt2.Rows[0]["TEL1"].ToString() + " / " + dt2.Rows[0]["TEL2"].ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            return "Error al procesar los datos de entrada, " + ex.Message + ".";
                        }
                    }
                    //----------------------------------------------------------------------
                    sSql = "SELECT DESCDOC FROM CAB_DOCUMENTACION WITH(NOLOCK) ";
                    sSql = sSql + " INNER JOIN GRAL_DOCUMENTACION WITH(NOLOCK) ON  GRAL_DOCUMENTACION.CODDOC = CAB_DOCUMENTACION.CODDOC ";
                    sSql = sSql + " WHERE CAB_DOCUMENTACION.IdCAB= " + xIDCabecera + " AND DOCASOC = 'S'";

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

                                xDescDoc = dt3.Rows[0]["DESCDOC"].ToString();

                            }
                        }
                        catch (Exception ex)
                        {
                            return "Error al procesar los datos de entrada, " + ex.Message + ".";
                        }
                    }
                    //----------------------------------------------------------------------
                    sSql = "SELECT DESCDOC FROM CAB_DOCUMENTACION WITH(NOLOCK) ";
                    sSql = sSql + " INNER JOIN GRAL_DOCUMENTACION  WITH(NOLOCK)  ON CAB_DOCUMENTACION.CODDOC = GRAL_DOCUMENTACION.CODDOC";
                    sSql = sSql + " WHERE DOCASOC = 'N' AND IDCAB = " + xIDCabecera;

                    using (SqlCommand cmd4 = new SqlCommand(sSql, con))
                    {
                        DataTable dt4 = new DataTable();
                        try
                        {

                            SqlDataAdapter da4 = new SqlDataAdapter(cmd4);
                            da4.Fill(dt4);
                            int Contador = dt4.Rows.Count;
                            if (dt4.Rows.Count > 0)
                            {

                                xDescDoc2 = dt4.Rows[0]["DESCDOC"].ToString();

                            }
                        }
                        catch (Exception ex)
                        {
                            return "Error al procesar los datos de entrada, " + ex.Message + ".";
                        }
                    }
                    //----------------------------------------------------------------------
                    sSql = "SELECT CODCONC, PATOL_CODIF FROM dbo.GRAL_CONCEPTOS  WITH(NOLOCK) WHERE CODCONC = '" + xConcepto + "' ORDER BY CODCONC";
                    using (SqlCommand cmd5 = new SqlCommand(sSql, con))
                    {
                        DataTable dt5 = new DataTable();
                        try
                        {

                            SqlDataAdapter da5 = new SqlDataAdapter(cmd5);
                            da5.Fill(dt5);
                            int Contador = dt5.Rows.Count;
                            if (dt5.Rows.Count > 0)
                            {

                                xPatolCodif = dt5.Rows[0]["PATOL_CODIF"].ToString();

                            }
                        }
                        catch (Exception ex)
                        {
                            return "Error al procesar los datos de entrada, " + ex.Message + ".";
                        }
                    }
                    //----------------------------------------------------------------------
                    sSql = "SELECT DESCMEDIC,DOSIS FROM CAB_MEDICAMENTOS WITH(NOLOCK) ";
                    sSql = sSql + " INNER JOIN GRAL_MEDICAMENTOS WITH(NOLOCK) ON CAB_MEDICAMENTOS.CODMEDIC = GRAL_MEDICAMENTOS.CODMEDIC ";
                    sSql = sSql + " WHERE IDCAB = " + xIDCabecera;

                    using (SqlCommand cmd6 = new SqlCommand(sSql, con))
                    {
                        DataTable dt6 = new DataTable();
                        try
                        {

                            SqlDataAdapter da6 = new SqlDataAdapter(cmd6);
                            da6.Fill(dt6);
                            int Contador = dt6.Rows.Count;
                            if (dt6.Rows.Count > 0)
                            {
                                xDescDosis = dt6.Rows[0]["DESCMEDIC"].ToString() + " / " + dt6.Rows[0]["DOSIS"].ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            return "Error al procesar los datos de entrada, " + ex.Message + ".";
                        }
                    }
                    //----------------------------------------------------------------------
                    sSql = "SELECT DESCOS FROM GRAL_OBRASSOCIALES WITH(NOLOCK)  WHERE CODOS ='0'";

                    using (SqlCommand cmd7 = new SqlCommand(sSql, con))
                    {
                        DataTable dt7 = new DataTable();
                        try
                        {
                            SqlDataAdapter da7 = new SqlDataAdapter(cmd7);
                            da7.Fill(dt7);
                            int Contador = dt7.Rows.Count;
                            if (dt7.Rows.Count > 0)
                            {
                                xDescCos = dt7.Rows[0]["DESCOS"].ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            return "Error al procesar los datos de entrada, " + ex.Message + ".";
                        }
                    }
                    con.Close();
                }
            }
            return "";
        }       
    }
}





