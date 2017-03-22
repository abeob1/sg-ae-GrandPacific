Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Data
Imports System.IO
Imports System.Data.Odbc
Imports System.Data.Common
Imports Sap.Data.Hana

Module modCommon

#Region "GetSystemIntializeInfo"
    Public Function GetSystemIntializeInfo(ByRef oCompDef As CompanyDefault, ByRef sErrDesc As String) As Long

        ' **********************************************************************************
        '   Function    :   GetSystemIntializeInfo()
        '   Purpose     :   This function will be providing information about the initialing variables
        '               
        '   Parameters  :   ByRef oCompDef As CompanyDefault
        '                       oCompDef =  set the Company Default structure
        '                   ByRef sErrDesc AS String 
        '                       sErrDesc = Error Description to be returned to calling function
        '               
        '   Return      :   0 - FAILURE
        '                   1 - SUCCESS
        '   Author      :   Shibin
        '   Date        :   SEP 2016
        ' **********************************************************************************

        Dim sFuncName As String = String.Empty
        Dim sConnection As String = String.Empty
        Dim sSqlstr As String = String.Empty
        Try

            sFuncName = "GetSystemIntializeInfo()"
            Console.WriteLine("Starting System Intial  Function", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function", sFuncName)

            sFuncName = "GetSystemIntializeInfo()"
            ' Console.WriteLine("Starting Function", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function", sFuncName)


            oCompDef.sServer = String.Empty
            oCompDef.sDBUser = String.Empty
            oCompDef.sDBPwd = String.Empty
            oCompDef.sSourceDBName = String.Empty
            oCompDef.sTargetDBName = String.Empty
            oCompDef.sDebug = String.Empty

            oCompDef.sInputPath = String.Empty
            oCompDef.sFailPath = String.Empty
            oCompDef.sSuccessPath = String.Empty

            'PublicVariable.SourceConnection = System.Configuration.ConfigurationManager.ConnectionStrings("SourceConnection").ConnectionString

            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("SERVERNODE").ToString) Then
                oCompDef.sServer = ConfigurationManager.AppSettings("SERVERNODE").ToString
            End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("UID").ToString) Then
                oCompDef.sDBUser = ConfigurationManager.AppSettings("UID").ToString
            End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("PWD").ToString) Then
                oCompDef.sDBPwd = ConfigurationManager.AppSettings("PWD").ToString
            End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("SOURCECS").ToString) Then
                oCompDef.sSourceDBName = ConfigurationManager.AppSettings("SOURCECS").ToString
            End If
            ''If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("TARGETCS").ToString) Then
            ''    oCompDef.sTargetDBName = ConfigurationManager.AppSettings("TARGETCS").ToString
            ''End If

            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("SOURCESAPUser").ToString) Then
                oCompDef.sSourceSAPUser = ConfigurationManager.AppSettings("SOURCESAPUser").ToString
            End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("SOURCESAPPWD").ToString) Then
                oCompDef.sSourceSAPPwd = ConfigurationManager.AppSettings("SOURCESAPPWD").ToString
            End If

            ''If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("TARGETSAPUser").ToString) Then
            ''    oCompDef.sTargetSAPUser = ConfigurationManager.AppSettings("TARGETSAPUser").ToString
            ''End If
            ''If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("TARGETSAPPWD").ToString) Then
            ''    oCompDef.sTargetSAPPwd = ConfigurationManager.AppSettings("TARGETSAPPWD").ToString
            ''End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("DRIVER").ToString) Then
                oCompDef.sDriver = ConfigurationManager.AppSettings("DRIVER").ToString
            End If

            ' folder

            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("Debug")) Then
                oCompDef.sDebug = ConfigurationManager.AppSettings("Debug")
            End If

            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("LogPath")) Then
                oCompDef.sFilepath = ConfigurationManager.AppSettings("LogPath")
            End If

            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("InputPath")) Then
                oCompDef.sInputPath = ConfigurationManager.AppSettings("InputPath")
            End If


            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("SuccessPath")) Then
                oCompDef.sSuccessPath = ConfigurationManager.AppSettings("SuccessPath")
            End If

            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("FailPath")) Then
                oCompDef.sFailPath = ConfigurationManager.AppSettings("FailPath")
            End If

            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("FileName")) Then
                oCompDef.sFileName = ConfigurationManager.AppSettings("FileName")
            End If

            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("sSMTPServer")) Then
                oCompDef.sSMTPServer = ConfigurationManager.AppSettings("sSMTPServer")
            End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("sSMTPPort")) Then
                oCompDef.sSMTPPort = ConfigurationManager.AppSettings("sSMTPPort")
            End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("sSMTPUser")) Then
                oCompDef.sSMTPUser = ConfigurationManager.AppSettings("sSMTPUser")
            End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("sSMTPPassword")) Then
                oCompDef.sSMTPPassword = ConfigurationManager.AppSettings("sSMTPPassword")
            End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("sEmailFrom")) Then
                oCompDef.sEmailFrom = ConfigurationManager.AppSettings("sEmailFrom")
            End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("sAEmailID")) Then
                oCompDef.sToEmailID = ConfigurationManager.AppSettings("sAEmailID")
            End If
            If Not String.IsNullOrEmpty(ConfigurationManager.AppSettings("sSSL")) Then
                oCompDef.sSSL = ConfigurationManager.AppSettings("sSSL")
            End If

            Console.WriteLine("System Intial is Completed with SUCCESS ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with SUCCESS", sFuncName)
            GetSystemIntializeInfo = RTN_SUCCESS

        Catch ex As Exception
            WriteToLogFile(ex.Message, sFuncName)
            Console.WriteLine("Completed with ERROR ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("System Intial is Completed with ERROR", sFuncName)
            GetSystemIntializeInfo = RTN_ERROR
        End Try
    End Function
#End Region

#Region "Connect To Company"
    Public Function ConnectToCompany(ByVal Connection As Array, ByVal company As SAPbobsCOM.Company, ByRef sErrDesc As String) As Long

        ' **********************************************************************************
        '   Function    :   ConnectToCompany()
        '   Purpose     :   This function will connect to Source Company
        '               
        '   Parameters  :   ByVal Connection As Array
        '                       Connection =  set the Connection String
        '                   ByVal company As SAPbobsCOM.Company
        '                       company = set the Company
        '                   ByRef sErrDesc AS String 
        '                       sErrDesc = Error Description to be returned to calling function
        '               
        '   Return      :   0 - FAILURE
        '                   1 - SUCCESS
        '   Author      :   Shibin
        '   Date        :   SEP 2016
        ' **********************************************************************************
        Dim sErrMsg As String = ""
        Dim sErrCode As Integer
        Dim sFuncName As String = String.Empty
        Try
            sFuncName = "ConnectToCompany()"
            If company.Connected Then
                company.Disconnect()
            End If            

            company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB
            company.CompanyDB = Connection(0).ToString()
            company.Server = Connection(2).ToString()
            company.DbUserName = Connection(3).ToString()
            company.DbPassword = Connection(4).ToString()
            company.UserName = Connection(5).ToString
            company.Password = Connection(6).ToString
            company.UseTrusted = False
            sErrDesc = String.Empty

            If company.Connect <> 0 Then
                company.GetLastError(sErrCode, sErrMsg)
                sErrDesc = sErrMsg
                If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Source Company not connected " & Connection(0).ToString() & " - " & sErrMsg, "ConnectToCompany()")
                Console.WriteLine("Source Company not connected ", sFuncName)
            Else
                Console.WriteLine("Source Company connected  successfully ", sFuncName)
                If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Source Company connected  successfully " & Connection(0).ToString(), "ConnectToCompany()")
            End If
            Return sErrCode
        Catch ex As Exception
            'WriteLog("SystemInitial: " + ex.ToString)
            Return ex.ToString
        End Try

    End Function

    
#End Region

#Region "Execute HANA Query- Datatable"
    Public Function HANAtoDatatable(sQuery As String, ByRef sErrDesc As String) As DataTable
        '  **********************************************************************************
        '    Function    :   HANAtoDatatable()
        '    Purpose     :   This function will fetch the information based on the query and fill the Datatable
        '                
        '    Parameters  :  
        '                    ByRef sErrDesc AS String 
        '                        sErrDesc = Error Description to be returned to calling function
        '                
        '    Return      :   0 - FAILURE
        '                    1 - SUCCESS
        '    Author      :   Shibin
        '    Date        :   Sep 2016
        '  *********************************************************************************


        Dim sFuncName As String = "HANAtoDatatable"
        Dim oDataset As New DataSet()
        'Dim sConnString As String = System.Configuration.ConfigurationManager.ConnectionStrings("SourceHanaConnection").ConnectionString
        'Dim oHanaOdbcConnection As New OdbcConnection(sConnString)
        'Dim oHanaConnection As HanaConnection = New HanaConnection(sConnString)

        Dim sConstr As String = "DRIVER=" & p_oCompDef.sDriver & ";UID=" & p_oCompDef.sDBUser & ";PWD=" & p_oCompDef.sDBPwd & ";SERVERNODE=" & p_oCompDef.sServer & ";CS=" & p_oCompDef.sSourceDBName
        Dim oHanaOdbcConnection As New OdbcConnection(sConstr)
        Dim oHanaOdbcCommand As New OdbcCommand()
        Dim oHanaConnection As HanaConnection = New HanaConnection(sConstr)
        Try
            sFuncName = "HANAtoDatatable()"

            'Console.WriteLine("Starting Hana Query ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("HANA Connection", sFuncName)

            If oHanaConnection.State = ConnectionState.Closed Then
                oHanaConnection.Open()
            End If
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Successfully HANA Connection done and Query passed..........", sFuncName)
            Dim cmd As HanaCommand = New HanaCommand(sQuery, oHanaConnection)
            'Dim reader As HanaDataReader = cmd.ExecuteReader()
            Dim oHanaDA As New HanaDataAdapter(cmd)
            oHanaDA.Fill(oDataset)
            oHanaDA.Dispose()
            'reader.Close()
            cmd.Dispose()

            Return oDataset.Tables(0)

            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Successfully Query retreived result..........", sFuncName)

        Catch Ex As Exception
            sErrDesc = Ex.Message
            'Console.WriteLine("Completed with ERROR ", sFuncName)
            WriteToLogFile(Ex.Message, sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Error in HANA Connection/Query passed..........", sFuncName)
            Throw New Exception(Ex.Message)
        Finally
            oHanaConnection.Close()
            oHanaConnection.Dispose()
        End Try
    End Function
#End Region


    Public Function ConvertRecordset(ByVal SAPRecordset As SAPbobsCOM.Recordset, ByRef sErrDesc As String) As DataTable

        '\ This function will take an SAP recordset from the SAPbobsCOM library and convert it to a more
        '\ easily used ADO.NET datatable which can be used for data binding much easier.
        Dim sFuncName As String = String.Empty

        Dim dtTable As New DataTable
        Dim NewCol As DataColumn
        Dim NewRow As DataRow
        Dim ColCount As Integer

        Try
            sFuncName = "ConvertRecordset()"
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function ", sFuncName)

            For ColCount = 0 To SAPRecordset.Fields.Count - 1
                NewCol = New DataColumn(SAPRecordset.Fields.Item(ColCount).Name)
                dtTable.Columns.Add(NewCol)
            Next

            Do Until SAPRecordset.EoF

                NewRow = dtTable.NewRow
                'populate each column in the row we're creating
                For ColCount = 0 To SAPRecordset.Fields.Count - 1

                    NewRow.Item(SAPRecordset.Fields.Item(ColCount).Name) = SAPRecordset.Fields.Item(ColCount).Value

                Next

                'Add the row to the datatable
                dtTable.Rows.Add(NewRow)


                SAPRecordset.MoveNext()
            Loop

            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with SUCCESS", sFuncName)
            Return dtTable

        Catch ex As Exception
            ConvertRecordset = Nothing
            sErrDesc = ex.Message
            'Console.WriteLine("Completed with ERROR ", sFuncName)
            WriteToLogFile(ex.Message, sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with ERROR" & ex.Message, sFuncName)

            Throw New Exception(ex.Message)
            Exit Function
        End Try


    End Function





End Module


