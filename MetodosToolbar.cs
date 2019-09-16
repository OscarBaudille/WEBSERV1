using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;


namespace WcfServiceAPE
{
     [ServiceContract(Namespace = "http://ar.com.osde/osgapeservice/")]
    public interface MetodosToolbar
    {
        // buscarArchivos
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string buscarArchivos(string xIDCabecera, string xnroAfiliado, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargarPrestadoresAnexos(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string agregarArchivos(string xAfiliado, string xTipo, string xRuta, string xArchivo, string xComentario, string xUsuario, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string envioSedeCentral(long[] idCabecera, string[] estadoAdm, string usuario, string modulo, string filialLogin, string bd);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string verNotificacionesExp(long idCabecera, string usuario, string modulo, string filialLogin, string bd);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string buscarComentarios(long idCabecera, string usuario, string modulo, string filialLogin, string bd);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string ingresarComent(long idCabecera, string xComentario, string xUsuario, string usuario, string modulo, string filialLogin, string bd);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string Filialesactivas(string xIDCabecera, string xnroAfiliado, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string DerivarLegajo(string xIDCabecera, string xFderiva, string xUbcAdm, string xEstadoCble, string xModulo, string xBase, string xUserLog, string xFilialLog);

        //----24/08/2018--------------------------------------------------
        // TONY LO TERMINA...
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string ImprimirCaratula(long xIDCabecera, string xUserLog, string xModulo, string xFilialLog, string xBase);
    }
}