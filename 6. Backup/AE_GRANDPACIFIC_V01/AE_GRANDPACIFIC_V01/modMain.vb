Module modMain
#Region "Company Default"
    Public Structure CompanyDefault

        Public sServer As String
        Public sSourceDBName As String
        Public sTargetDBName As String
        Public sDBUser As String
        Public sDBPwd As String
        Public sSourceSAPUser As String
        Public sTargetSAPUser As String
        Public sSourceSAPPwd As String
        Public sTargetSAPPwd As String
        Public sDriver As String

        Public sDebug As String
        Public sFilepath As String
        Public sInputPath As String
        Public sFailPath As String
        Public sSuccessPath As String
        Public sFileName As String

        Public sSMTPServer As String
        Public sSMTPPort As String

        Public sSMTPUser As String
        Public sSMTPPassword As String      
        Public sEmailFrom As String
        Public sToEmailID As String
        Public sSSL As String


    End Structure
#End Region

#Region "Global Variable"
    ' Return Value Variable Control
    Public Const RTN_SUCCESS As Int16 = 1
    Public Const RTN_ERROR As Int16 = 0
    ' Debug Value Variable Control
    Public Const DEBUG_ON As Int16 = 1
    Public Const DEBUG_OFF As Int16 = 0

    ' Global variables group
    Public p_iDebugMode As Int16 = DEBUG_ON
    Public p_iErrDispMethod As Int16
    Public p_iDeleteDebugLog As Int16
    Public p_oCompDef As CompanyDefault
    Public p_dProcessing As DateTime
    Public p_oDtSuccess As DataTable
    Public p_oDtError As DataTable
    Public p_SyncDateTime As String
    Public p_SrcCompany As SAPbobsCOM.Company
    Public p_oCompany As SAPbobsCOM.Company
    Public oTrgIMasterCompany As New SAPbobsCOM.Company
#End Region

    Sub Main()

        Dim sFuncName As String = String.Empty
        Dim sErrDesc As String = String.Empty

        Dim strConnection As Array
        Dim sApp As String = String.Empty

        Try
            sFuncName = "Main'"
            Console.WriteLine("Starting Main Function", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function", sFuncName)

            p_oCompany = New SAPbobsCOM.Company
            If GetSystemIntializeInfo(p_oCompDef, sErrDesc) <> RTN_SUCCESS Then Throw New ArgumentException(sErrDesc)

            sApp = p_oCompDef.sSourceDBName & ";" & p_oCompDef.sTargetDBName & ";" & p_oCompDef.sServer & ";" & p_oCompDef.sDBUser & ";" & p_oCompDef.sDBPwd & ";" & p_oCompDef.sSourceSAPUser & ";" & p_oCompDef.sSourceSAPPwd & ";" & p_oCompDef.sTargetSAPUser & ";" & p_oCompDef.sTargetSAPPwd
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("App " & sApp, sFuncName)
            strConnection = sApp.Split(";")

            ' '''******************************************Connection  of Source & Target Company Started ******************************************
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Calling Function ConnectToCompany()", sFuncName)
            If (ConnectToCompany(strConnection, p_oCompany, sErrDesc) <> 0) Then
                Throw New Exception(sErrDesc)
            End If
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Source Connected SUCCESSFULLY ", sFuncName)


            ' '''******************************************Connection  of Source & Target Company Ended ******************************************


            Console.WriteLine("Calling IdentifyCSVFile() ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Calling IdentifyCSVFile() ", sFuncName)
            If IdentifyCSVFile(sErrDesc) <> RTN_SUCCESS Then Throw New ArgumentException(sErrDesc)

            Console.WriteLine("Completed With SUCCESS ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed With SUCCESS ", sFuncName)








        Catch ex As Exception
            Console.WriteLine("Synchronization Completed with ERROR ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Sync Completed with ERROR", sFuncName)
            Call WriteToLogFile(ex.Message, sFuncName)


        End Try

    End Sub



End Module
