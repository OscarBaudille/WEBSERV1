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
    public interface GrillaPrincipal    
    {
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string PopulateDropDownListAgrup(string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string PopulateDropDownListCptos(string value, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string PopulateDropDownListOsocial(string value, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string PopulateDropDownListFiliales(string value, string xModulo, string xBase, string xUserLog, string xFilialLog);
        // TODO: agregue aquí sus operaciones de servicio

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string PopulateDropDownListGrillaPrincipal(string xAgrup, string xCpto, string xOS, string xEstAdm, string xMesVto, string xAnioVto, string xEstCont, string xEstVto, string xAfil, string xAnioPrest,
               string xFechaSalida, string xFilialCreacion, string xFilialServicio, string xFilialContable, string xFilialUbicAdmin,
               string xNroAfiliado, string xNroSolicitudSur, string xExpedientes, string xNroId, string xFecreDesde, string xFecreHasta, string xOrden, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string PopulateDropDownListGrillaSinCabecera(string value, string xModulo, string xBase, string xUserLog, string xFilialLog);

        //Metodos para la tercer solapa...
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        [OperationContract]
        string PopulateDropDownListAfiliacion(string xAfil, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string PopulateDropDownListFcreacion(string xFcrea, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string PopulateDropDownListFservicio(string xFserv, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string PopulateDropDownListUbicCont(string xUcont, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]  
        string PopulateDropDownListUbicAdm(string xUadm, string xModulo, string xBase, string xUserLog, string xFilialLog);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string ExportarExcelID(string idCabeceras, string xModulo, string xBase, string xUserLog, string xFilialLog);  

      
    }
    // Utilice un contrato de datos, como se ilustra en el ejemplo siguiente, para agregar tipos compuestos a las operaciones de servicio.

   
    [DataContract]
    public class StatusWs
    {
        [DataMember]
        public string Status { get; set; }
    }

    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "OSDETRON ";
        object Ag1=null ;

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }

        [DataMember]
        public object objDept
        {
            get { return Ag1; }
            set { Ag1 = value; }
        }
      
    }

    [DataContract]
    //Manejo de mensajeria...
        public class BaseRespuesta
        {
            [DataMember]
            public string MensajeRespuesta { get; set; }
            [DataMember]
            public string Error { get; set; }
        }
  
}
