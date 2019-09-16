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
    public interface DocDiscapacidadABM
    {
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string validarCertificado(string xAfiliado, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string abmDocDisca(string xIDcabecera, string xIDDOC, string xuser, string xIDPapel, string xAccion, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargarCertificado(string xFilialLogin, string xNroafil, string xTransporte, string xFvenc, string xPatologia, string xFecert, string xIDcabecera, string xIDDOC, string xUsuario, string xIDPapel, string xAccion, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string prest_relacionado(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargar_habilitaciones(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargar_prestador(string xCUIT, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string nueva_habilitacion(string xFilial, string xCUIT, string xINSCRNP, string xModalidad, string xCATEG, string xHAB, string xFEVTO, string xUSUARIO, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string asignar_habilitacion(string xIDCABECERA, string xUsuario, string xIdHab, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string borrar_habilitacion(string xIDpapel, string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargar_modalidad(string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargar_pm(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargar_prestacion(string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string alta_PM(string xFilial, string xfechapm, string xIDcabecera, string xUsuario, string xPrestacion, string xReqcertal, string xTienecert, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string modificar_pm(string xidpm, string xUsuario, string xIDcabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargaEspecialidad(string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargaModalidad(string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string presupuestosCargados(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string buscarPrestadorPTO(string xCUIT, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string altaPresupuesto(string xFILIAL, string xCUIT, string xIDCabecera, string xUsuario, string xFepresup, string xModalidad, string xEspecialidad, string xImporte, string xUnidadMedida, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string bajaPresupuesto(string IDpresupuesto, string xUsuario, string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

    }
}