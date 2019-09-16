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
using System.Collections;


namespace WcfServiceAPE
{
     [ServiceContract(Namespace = "http://ar.com.osde/osgapeservice/")]
    public interface Subgrillas
    {
        // Llenado de Subgrillas
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string cons_con_presupuestos(string xIDCABECERA, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string cons_sin_presupuestos(string xIDCABECERA, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string doc_Dicapacidad(string xIDCABECERA, string xPeriodo, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string datos_ape(string xIDCABECERA, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string sgrilla_registronac(string xIDCABECERA, string xModulo, string xBase, string xUserLog, string xFilialLog);
        
        [OperationContract]
        string sgrilla_sinconsumo(string xIDCABECERA, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        string consumos_Adm(string xidcabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        string DocumentosAdmS(string xIDCABECERA, string xCONCEPTO, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        string DocumentosAdmN(string xIDCABECERA, string xCONCEPTO, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        string agregarDocumento(string xIDCABECERA, string xCONCEPTO, string[] xIDDOC, string[] xRESP_DOC, string xDOCASOC, string[] xChecked, string[] xFEPM, string xModulo, string xBase, string xUserLog, string xFilialLog);
     
        [OperationContract]
        string responsableDocCheck(string xIDCABECERA,string xIDDOC,string Responsable, string xModulo, string xBase, string xUserLog, string xFilialLog);
        //------------------------------------------------------------------------------------------------------------------
        [OperationContract]
        string AddPresupuestoCont(string xIDCabecera, string xCodMedic, string xConcepto, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        string RemPresupuestoCont(string xIDCabecera, string xCodMedic, string xConcepto, string xModulo, string xBase, string xUserLog, string xFilialLog);
        
        [OperationContract]
        string DatosCreayModCont(string xIDReg, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        string VisualizarLogCons(string xIDReg, string xModulo, string xBase, string xUserLog, string xFilialLog);
        
        [OperationContract]
        string EliminarDocumentoCont(string xIDReg, string xModulo, string xBase, string xUserLog, string xFilialLog);
        
        [OperationContract]
        string CargarFormModif(long xIDReg, string xConcepto, string xTipo, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        string DocContable(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        string DocumentosContables(string xIDCabecera, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        string MedicamentosContPendyObt(string xIDCabecera, string xConcepto, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        string ModificacionConsumo(string xIDCab , string xIDCons , string xConcepto , string xTipo , string xNroAfiliado  , string xNroAfiliado2,string xFePrest , string xOrdenPractica  , string xNroTramite  , string xFeCierre , string xImportePrest, string xNroFaC,string xFeFactura,string xImporte,string xNroRecibo,string xFeRecibo,string xFeDebito,string xCodPrestador,string xNombrePrest,string xNroRemito,string xFeRemito,string xNroEgreso,string xFeEgreso,string xNroCheque,string xBanco,string xCuenta,string xCodEsp,string xModulo, string xBase, string xUserLog, string xFilialLog , string xHayModifRecibo,string[] xNroReng,string[] xAnticipo,string[] xEstado,string[] xNroReng2,string[] xCant2,string[] xIdMed, string[] xPrecio,string[] xEstado2 );
    }

}