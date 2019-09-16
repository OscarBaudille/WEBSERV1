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
    public interface MenuABM
    {
        // ABM Habilitaciones
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string modifDiscapacidad(string xCUIT, string xApellido, string xNombre, string xTelefono1, string xEmail, string xCodEsp, string xTipo, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string altaDiscapacidad(string xCUIT, string xApellido, string xNombre, string xTelefono1, string xEmail, string xCodEsp, string xTipo, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string consultaDiscapacidad(string xCUIT, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string datosDiscapacidad(string xModulo, string xBase, string xUserLog, string xFilialLog);

        //ABM Afiliados
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string buscar_afiliados(string xnroAfil, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargaCAP(string xfilDebito,  string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargaPlan(string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string alta_afiliado(string xFilial, string xnroAfil, string xBeneficio, string xNombre, string xApellido, string xSexo, string xDni, string xFechnac, string xCodhiv, string xFildeb, string xTelefonoper, string xTelefonoalt, string xCap, string xPlan, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string mod_afiliado(string xFilial, string xnroAfil, string xBeneficio, string xNombre, string xApellido, string xSexo, string xDni, string xFechnac, string xCodhiv, string xFildeb, string xTelefonoper, string xTelefonoalt, string xCap, string xPlan, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        //string abm_rnos(string xFilial, string xnroAfil, string xBeneAfil, string[] xFedesde, string[] xFehasta, string[] xRnos, string[] xEstado, string xModulo, string xBase, string xUserLog, string xFilialLog);
        string abm_rnos(string xFilial, string xnroAfil, string xBeneAfil, string[] xFedesde, string[] xFehasta, string[] xRnos,  string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string eliminar_rnos(string xfilAfil, string xnroAfil, string xbeneAfil, string xModulo, string xBase, string xUserLog, string xFilialLog);

        //ABM Conceptos
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string conceptosactivos(string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string tipodocumento(string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string obtenerdocument(string xnrodoc, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string altadocasociado(string xcodigo, string xconcepto, string xdescripcion, string xtipodoc, string xrespdoc, string xestado, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string modifdocumento(string xcodigo, string xconcepto, string xdescripcion, string xtipodoc, string xestado, string xrespdoc, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string buscar_motivos(string xtipomotivo, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string altamotivo(string xtipomotivo, string xdesclarg, string xdescorta, string xestado, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string mod_motivos(string xcodanul, string xdesclarg, string xdescorta, string xestado, string xtipomotivo, string xModulo, string xBase, string xUserLog, string xFilialLog);
       
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string tipo_medicamento(string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string filtrar_grillamedic(string xcodmed, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string buscar_medicamentos(string xcodmed, string xcodconc, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string ver_tope(string xcodmed, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string abm_medicamentos(string xcodmed, string xcodconc, string xdescmed, string xobs, string xtipomed, string xest, string xcodlarg, string modCod, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string abm_topes(string xcodmed, string xcodconc, string ximporte, string xfedesde, string xfehasta, string xIdtope, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string cargarRNOS(string xfilAfil, string xnroAfil, string xbeneAfil, string xModulo, string xBase, string xUserLog, string xFilialLog);
    
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string BuscarAfiliadoCab(string xnroAfil, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string validateRnosCab(string xnroAfil, string xFdesde, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string validateModulo(string xCodconc, string xFdesde, string xFhasta,string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string ObtenerMedicamentosCab(string xCodconc, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string ObtenerDocumentosCab(string xNroAfiliado, string xCodconc, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string AltaCabecera(string xNroAfiliado, string xName, string xOs,string xPlan,string xFildeb,string xCap, 
            string xFdesde,string xFhasta,string xTipoexp,string xCodconc, string xModcab,string xAfiliacion, string[] xCantmed,
            string[] xCodmed, string[] xMedicamento, string[] responsable, string[] xCoddoc, string[] xRespdoc,
            string[] xDescdoc, string xFepm, string[] xEstdoc, string xObs, string xUser, string idSur,
            string xModulo, string xBase, string xUserLog, string xFilialLog);
        //-------------------------------------------------------------------------------------------------------------------------
        
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string PrestadorConsumo(string xCodpres,string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string altaPrestadorCON(string xCodpres, string xNombre, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string CbtEgresoConsumos(string xCBTEEGRESO, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string BuscarBancoComsumo(string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string AltaConsumoGeneral(string xNroAfiliado, string xNombreAfil, string xNroFAC, string xFechaFact, string xImporteFAC,
            string xImpoorteFacPres, string xNotaCred, string xCodPrestador, string xnombrePrest, string xNroRemito, string xFeRemito,
            string xConcepto, string xFechaPrestacion, string xOrPractica, string xTramite, string xFechaCierre, string xCbteEgreso,
            string xFechaPago, string xNroCheque, string xCodBanco, string xNombrebanco, string xNroCuenta, string xNroRecibo,
            string xFechaRecibo, string xFechaDebito, string[] xIDAnticipo, string[] xAnticipos, string[] xNroReg, string[] xCantmed,
            string[] xCodMedic, string[] xPrecioUN, string xSaldoFC, string xModulo, string xBase, string xUserLog, string xFilialLog);
        
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string EliminarConsumoDiscapacidad(string xIDreg, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string AltaConsumoDiscapacidad(string xNroAfiliado, string xCodPrestador, string xnombrePrest,
            string xNroFAC, string xFechaFact, string xImporteFAC, string xImpoorteFacPres,
            string xNroRecibo, string xFechaRecibo, string xFechaPrestacion,
            string xTramite, string xReintCAP, string xReintNRO,
            string xCbteEgreso, string xFechaEgreso, string xNroCheque, string xCodBanco, string xNombrebanco,
            string xNroCuenta, string[] xIDAnticipo, string[] xAnticipo,string xModulo, string xBase, string xUserLog, string xFilialLog);
       
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string VerNotificaciones(string xModulo, string xBase, string xUserLog, string xFilialLog);
        
        //----24/08/2018--------------------------------------------------
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string ExpEnvioSedeCentral(string xFeDesde, string xFeHasta, string usuario, string modulo, string bd, string filialLogin);

        //----27/09/2018--------------------------------------------------
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string ConceptosHabilitados(string usuario, string modulo, string bd, string filialLogin);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string ReaperturaEjecutar(string Concepto,string MesVto,string Aniovto,string usuario, string modulo, string bd, string filialLogin);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string GrabarReApertura(string ID,string Concepto,string NroAfil,string MesVto ,string Aniovto,string CantMes,string CabPos,string usuario,string modulo,string bd,string FilialLogin);


    }
       
}