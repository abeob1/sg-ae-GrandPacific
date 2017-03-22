Imports System.IO
Imports System.IO.FileInfo
Imports System.Globalization
Imports System.Net.Mail
Imports System.Text
Imports SAPbobsCOM

Module modJournalVoucher

#Region "Identify CSV File"
    Public Function IdentifyCSVFile(ByRef sErrDesc As String) As Long

        ' **********************************************************************************
        '   Function    :   IdentifyCSVFile()
        '   Purpose     :   This function will identify the CSV file of Journal Entry
        '                    Upload the file into Dataview and provide the information to post transaction in SAP.
        '                     Transaction Success : Move the CSV file to SUCESS folder
        '                     Transaction Fail :    Move the CSV file to FAIL folder and send Error notification to concern person
        '               
        '   Parameters  :   ByRef sErrDesc AS String 
        '                       sErrDesc = Error Description to be returned to calling function
        '               
        '   Return      :   0 - FAILURE
        '                   1 - SUCCESS
        '   Author      :   JOHN
        '   Date        :   NOV 2016 
        ' **********************************************************************************


        Dim sSqlstr As String = String.Empty
        Dim bJEFileExist As Boolean
        Dim sFileType As String = String.Empty
        Dim oDTDistinct As DataTable = Nothing
        Dim oDTRowFilter As DataTable = Nothing
        Dim oDVJE As DataView = Nothing
        Dim oDVML As DataView = Nothing
        Dim oDVMR As DataView = Nothing
        Dim oDVST As DataView = Nothing
        Dim oDVIMPSTS As DataView = Nothing
        Dim oDICompany() As SAPbobsCOM.Company = Nothing
        Dim sCompanyDB As String = String.Empty
        Dim oDT_Entity As DataTable = Nothing
        Dim sFuncName As String = String.Empty
        Dim sFileName As String = String.Empty
        Dim oDTGLMAp As DataTable = Nothing
        Dim oDTUDT_ML As DataTable = Nothing
        Dim oDTUDT_ST As DataTable = Nothing
        Dim oDTUDT_MR As DataTable = Nothing
        Dim oDTDim3 As DataTable = Nothing
        Dim oDTDim4 As DataTable = Nothing
        Dim oDTSAPDim3 As DataTable = Nothing
        Dim oDT_MailText As DataTable = Nothing

        Try
            sFuncName = "IdentifyCSVFile()"
            Console.WriteLine("Starting Function", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function ", sFuncName)
            oDTGLMAp = New DataTable()
            oDTDim3 = New DataTable()
            oDTDim4 = New DataTable()
            oDTSAPDim3 = New DataTable()

            oDTUDT_ML = New DataTable()
            oDTUDT_ST = New DataTable()
            oDTUDT_MR = New DataTable()

            oDT_MailText = New DataTable()

            oDTGLMAp.Columns.Add("Code", GetType(String))
            oDTDim3.Columns.Add("Code", GetType(String))
            oDTDim4.Columns.Add("Code", GetType(String))



            'oDT_MailText.Columns.Add("SrNo", GetType(Int32))
            'oDT_MailText.Columns("SrNo").AutoIncrement = True
            'oDT_MailText.Columns("SrNo").AutoIncrementSeed = 1
            'oDT_MailText.Columns("SrNo").AutoIncrementStep = 1

            oDT_MailText.Columns.Add("FileName", GetType(String))
            oDT_MailText.Columns.Add("Status", GetType(String))
            oDT_MailText.Columns.Add("ErrorDescription", GetType(String))


            Dim DirInfo As New System.IO.DirectoryInfo(p_oCompDef.sInputPath)
            Dim SuccessDirInfo As New System.IO.DirectoryInfo(p_oCompDef.sSuccessPath)
            Dim FaliureDirInfo As New System.IO.DirectoryInfo(p_oCompDef.sFailPath)
            Dim sFilePath As String = SuccessDirInfo.ToString
            Dim fFilePath As String = FaliureDirInfo.ToString
            Dim iFilePath As String = DirInfo.ToString
            Dim files() As System.IO.FileInfo

            'files = DirInfo.GetFiles("" & p_oCompDef.sFileName & "*.csv")
            files = DirInfo.GetFiles("*.csv")
            oDT_MailText.Clear()
            For Each File As System.IO.FileInfo In files
                bJEFileExist = True
                oDTGLMAp.Clear()
                oDTDim3.Clear()
                oDTDim4.Clear()

                oDTUDT_ML.Clear()
                oDTUDT_ST.Clear()
                oDTUDT_MR.Clear()

                sFileName = File.Name
                sErrDesc = String.Empty
                Console.WriteLine("Attempting File Name - " & File.Name, sFuncName)
                If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Attempting File Name - " & File.Name, sFuncName)
                'sFileType = Replace(File.Name, ".txt", "").Trim
                'upload the CSV to Dataview

                ' '' ''Journal Voucher File - RV
                If sFileName.StartsWith("RV") Then
                    Console.WriteLine("GetDataViewFromTXT() ", sFuncName)
                    If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("GetDataViewFromTXT() ", sFuncName)
                    oDVJE = GetDataViewFromCSV(File.FullName, File.Name, oDTGLMAp, oDTDim3, oDTDim4, sErrDesc)

                    ' '' ''Statistic Tables - File - ML
                ElseIf sFileName.StartsWith("ML") Then
                    Console.WriteLine("GetDataViewFromCSV_ML() ", sFuncName)
                    If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("GetDataViewFromCSV_ML() ", sFuncName)
                    oDVML = GetDataViewFromCSV_ML(File.FullName, File.Name, oDTUDT_ML, sErrDesc)

                    ' '' ''Statistic Tables - File - ST
                ElseIf sFileName.StartsWith("ST") Then
                    Console.WriteLine("GetDataViewFromCSV_ST() ", sFuncName)
                    If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("GetDataViewFromCSV_ST() ", sFuncName)
                    oDVST = GetDataViewFromCSV_ST(File.FullName, File.Name, oDTUDT_ST, sErrDesc)

                    ' '' ''Statistic Tables - File - ST
                ElseIf sFileName.StartsWith("MR") Then
                    Console.WriteLine("GetDataViewFromCSV_MR() ", sFuncName)
                    If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("GetDataViewFromCSV_MR() ", sFuncName)
                    oDVMR = GetDataViewFromCSV_MR(File.FullName, File.Name, oDTUDT_MR, sErrDesc)
                End If


                ''  oDTDistinct = oDVJE.Table.DefaultView.ToTable(True, "Entity")
                If sErrDesc.Length > 0 Then
                    Console.WriteLine("Moving CSV file to Fail folder", sFuncName)
                    MoveFile(fFilePath, iFilePath, File.Name)
                    oDT_MailText.Rows.Add(File.Name, "Error", sErrDesc)
                    If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling MoveFile() for moving CSV file to fail folder", sFuncName)
                    IdentifyCSVFile = RTN_ERROR
                    Exit Function
                End If

                ''--------------------- Validation for Journal Voucher
                'SuiteCode not defined in GL mapping table TBLGLMAP.Suite8Code, 
                '    Show error message ‘Code xxxx  is not defined in the mapping table’.
                ' If Column 6 not found in OPRC.PrcCode where OPRC.DimCode = 3, show error message ‘Code xxxx is not defined’.

                'If Column 7 not found in OPRC.PrcCode where OPRC.DimCode = 4, show error message ‘Code xxxx is not defined’.


                If oDTGLMAp.Rows.Count > 0 Or oDTDim3.Rows.Count > 0 Or oDTDim4.Rows.Count > 0 Then
                    'Write_Validation(oDTGLMAp, oDTDim3, oDTDim4, File.Name)
                    
                    If oDTGLMAp.Rows.Count > 0 Then
                        For Each odr As DataRow In oDTGLMAp.Rows                            
                            oDT_MailText.Rows.Add(File.Name, "Error", odr(0) & "mapping G/L account not defined in SAP")
                            WriteToLogFile(odr(0) & "mapping G/L account not defined in SAP", sFuncName)
                        Next
                    End If

                    If oDTDim3.Rows.Count > 0 Then
                        For Each odr As DataRow In oDTDim3.Rows                            
                            oDT_MailText.Rows.Add(File.Name, "Error", odr(0) & " Code not defined in SAP")
                            WriteToLogFile(odr(0) & " Code not defined in SAP", sFuncName)
                        Next
                    End If

                    If oDTSAPDim3.Rows.Count > 0 Then
                        For Each odr As DataRow In oDTSAPDim3.Rows
                            oDT_MailText.Rows.Add(File.Name, "Error", odr(0) & " Code not defined in SAP")
                            WriteToLogFile(odr(0) & " Code not defined in SAP", sFuncName)
                        Next
                    End If

                    If oDTDim4.Rows.Count > 0 Then
                        For Each odr As DataRow In oDTDim4.Rows
                            oDT_MailText.Rows.Add(File.Name, "Error", odr(0) & " Code not defined in SAP")
                            WriteToLogFile(odr(0) & " Code not defined in SAP", sFuncName)
                        Next
                    End If
                    If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling MoveFile() for moving CSV file to fail folder", sFuncName)
                    MoveFile(fFilePath, iFilePath, File.Name)
                    Console.WriteLine("Completed With ERROR", sFuncName)
                    If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed With ERROR", sFuncName)
                    'oDT_MailText.Rows.Add(File.Name, "Validation Error", "Code not Defined - Check Log Folder")
                    Continue For
                End If

                If sFileName.StartsWith("RV") Then
                    If JournalVoucher_Posting(oDVJE, File.Name, sErrDesc) <> RTN_SUCCESS Then
                        MoveFile(fFilePath, iFilePath, File.Name)
                        Console.WriteLine("Moving CSV file to Fail folder", sFuncName)
                        If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling MoveFile() for moving CSV file to Fail folder", sFuncName)
                        oDT_MailText.Rows.Add(File.Name, "Error", sErrDesc)
                    Else
                        MoveFile(sFilePath, iFilePath, File.Name)
                        Console.WriteLine("Moving CSV file to Success folder", sFuncName)
                        If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling MoveFile() for moving CSV file to Success folder", sFuncName)
                        oDT_MailText.Rows.Add(File.Name, "Success", sErrDesc)
                    End If

                ElseIf sFileName.StartsWith("ML") Then
                    If StatisticRoom(oDVML, File.Name, sErrDesc) <> RTN_SUCCESS Then
                        Console.WriteLine("Moving CSV file to Fail folder", sFuncName)
                        MoveFile(fFilePath, iFilePath, File.Name)
                        If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling MoveFile() for moving CSV file to Fail folder", sFuncName)
                        oDT_MailText.Rows.Add(File.Name, "Error", sErrDesc)
                    Else
                        Console.WriteLine("Moving CSV file to Success folder", sFuncName)
                        MoveFile(sFilePath, iFilePath, File.Name)
                        If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling MoveFile() for moving CSV file to Success folder", sFuncName)
                        oDT_MailText.Rows.Add(File.Name, "Success", sErrDesc)
                    End If

                ElseIf sFileName.StartsWith("ST") Then
                    If FBCover(oDVST, File.Name, sErrDesc) <> RTN_SUCCESS Then
                        Console.WriteLine("Moving CSV file to Fail folder", sFuncName)
                        If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling MoveFile() for moving CSV file to Fail folder", sFuncName)
                        MoveFile(fFilePath, iFilePath, File.Name)
                        oDT_MailText.Rows.Add(File.Name, "Error", sErrDesc)
                    Else
                        Console.WriteLine("Moving CSV file to Success folder", sFuncName)
                        If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling MoveFile() for moving CSV file to Success folder", sFuncName)
                        MoveFile(sFilePath, iFilePath, File.Name)
                        oDT_MailText.Rows.Add(File.Name, "Success", sErrDesc)
                    End If

                ElseIf sFileName.StartsWith("MR") Then
                    If RoomRevenue(oDVMR, File.Name, sErrDesc) <> RTN_SUCCESS Then
                        Console.WriteLine("Moving CSV file to Fail folder", sFuncName)
                        If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling MoveFile() for moving CSV file to Fail folder", sFuncName)
                        MoveFile(fFilePath, iFilePath, File.Name)
                        oDT_MailText.Rows.Add(File.Name, "Error", sErrDesc)
                    Else
                        Console.WriteLine("Moving CSV file to Success folder", sFuncName)
                        If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling MoveFile() for moving CSV file to Success folder", sFuncName)
                        MoveFile(sFilePath, iFilePath, File.Name)
                        oDT_MailText.Rows.Add(File.Name, "Success", sErrDesc)
                    End If

                End If

            Next
            'oDT_MailText.Rows.Add("File1", "Success", " ")
            'oDT_MailText.Rows.Add("File2", "Success", " ")
            ''oDT_MailText.Rows.Add("File3", "Error", "Update the exchange error")
            'oDT_MailText.Rows.Add("File3", "Success", " ")
            'oDT_MailText.Rows.Add("File4", "Success", "")
            'oDT_MailText.Rows.Add("File5", "Success", "")
            If oDT_MailText.Rows.Count > 0 Then
                Console.WriteLine("Sending Email Notification.....", sFuncName)
                If p_iDebugMode = DEBUG_ON Then WriteToLogFile_Debug("Calling SendEmailNotification() for Email Notification", sFuncName)
                SendEmailNotification(oDT_MailText)
            End If

            Console.WriteLine("Completed With SUCCESS ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed With SUCCESS", sFuncName)
            IdentifyCSVFile = RTN_SUCCESS

        Catch ex As Exception
            sErrDesc = ex.Message
            WriteToLogFile(ex.Message, sFuncName)
            Console.WriteLine("Completed With ERROR", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed With ERROR", sFuncName)
            IdentifyCSVFile = RTN_ERROR
        End Try

    End Function
#End Region

#Region "Journal Voucher"
    Public Function GetDataViewFromCSV(ByVal CurrFileToUpload As String, ByVal Filename As String, ByRef oDTMAP As DataTable, ByRef oDTDim3 As DataTable, ByRef oDTDim4 As DataTable, ByRef sErrDesc As String) As DataView

        ' **********************************************************************************
        '   Function    :   GetDataViewFromCSV()
        '   Purpose     :   This function will upload the data from CSV file to Dataview
        '   Parameters  :   ByRef CurrFileToUpload AS String 
        '                       CurrFileToUpload = File Name
        '   Author      :   JOHN
        '   Date        :   NOV 2016 
        ' **********************************************************************************

        Dim dv As DataView

        Dim sFuncName As String = String.Empty
        sErrDesc = String.Empty
        Dim dperioddate As Date
        Dim oSR As StreamReader
        Dim oDVGLMAP As DataView = Nothing
        Dim oDTGLMAP As DataTable = Nothing
        Dim oDTJV As DataTable = Nothing
        Dim oDTCC3 As DataTable = Nothing
        Dim oDVCC3 As DataView = Nothing
        Dim oDTCC4 As DataTable = Nothing
        Dim oDVCC4 As DataView = Nothing
        Dim oDTCC5 As DataTable = Nothing
        Dim oDVCC5 As DataView = Nothing
        Dim sSQLDim4 As String = String.Empty
        Dim sSQL As String = String.Empty
        Dim oRset As SAPbobsCOM.Recordset = Nothing
        Dim oRsetDim4 As SAPbobsCOM.Recordset = Nothing
        Dim oRsetDim3 As SAPbobsCOM.Recordset = Nothing
        Dim sGLCode As String = String.Empty
        Dim sGLName As String = String.Empty
        Dim sCC3 As String = String.Empty
        Dim sSAPCC3 As String = String.Empty
        Dim sCC4 As String = String.Empty
        Dim fullMonthName As DateTime
        Dim dDate As Date
        Dim sDate As String = String.Empty
        Dim dDebit As Double = 0.0
        Dim dCredit As Double = 0.0
        Dim sSplit() As String


        Try
            sFuncName = "GetDataViewFromCSV"
            Console.WriteLine("Starting Function", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function", sFuncName)
            oRset = p_oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oRsetDim4 = p_oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oRsetDim3 = p_oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oDTMAP.Clear()
            oDTDim3.Clear()
            oDTGLMAP = New DataTable()

            sSQL = "SELECT T0.""Code"", T0.""Name"", T0.""U_SAPAcctCode"", T0.""U_AcctName"" FROM ""@TBLGLMAP""  T0"
            oRset.DoQuery(sSQL)
            oDTGLMAP = ConvertRecordset(oRset, sErrDesc)
            oDVGLMAP = New DataView(oDTGLMAP)


            sSQL = "SELECT T0.""U_CCCode"",T0.""U_Column6"", T0.""U_CostCenterName"" FROM ""@TBLCCMAP""  T0"
            oDTCC3 = New DataTable
            oRset.DoQuery(sSQL)
            oDTCC3 = ConvertRecordset(oRset, sErrDesc)
            oDVCC3 = New DataView(oDTCC3)

            If sErrDesc.Length > 0 Then
                Throw New ArgumentException(sErrDesc)
            End If


            'sSQL = "SELECT T0.""PrcCode"", T0.""PrcName"" FROM OPRC T0 WHERE T0.""DimCode""  = 3"
            'oDTCC5 = New DataTable
            'oRsetDim3.DoQuery(sSQL)
            'oDTCC5 = ConvertRecordset(oRsetDim3, sErrDesc)
            'oDVCC5 = New DataView(oDTCC5)

            'If sErrDesc.Length > 0 Then
            '    Throw New ArgumentException(sErrDesc)
            'End If


            sSQLDim4 = "SELECT T0.""PrcCode"", T0.""PrcName"" FROM OPRC T0 WHERE T0.""DimCode""  = 4"
            oDTCC4 = New DataTable
            oRsetDim4.DoQuery(sSQLDim4)
            oDTCC4 = ConvertRecordset(oRsetDim4, sErrDesc)
            oDVCC4 = New DataView(oDTCC4)

            If sErrDesc.Length > 0 Then
                Throw New ArgumentException(sErrDesc)
            End If
            'The Datatable to Return
            oDTJV = New DataTable()

            oDTJV.Columns.Add("Pdate", GetType(String))
            oDTJV.Columns.Add("SuiteCode", GetType(String))
            oDTJV.Columns.Add("SuiteName", GetType(String))
            oDTJV.Columns.Add("GLCode", GetType(String))
            oDTJV.Columns.Add("GLName", GetType(String))
            oDTJV.Columns.Add("Debit", GetType(Decimal))
            oDTJV.Columns.Add("Credit", GetType(Decimal))
            oDTJV.Columns.Add("Dim3", GetType(String))
            oDTJV.Columns.Add("Dim4", GetType(String))
            oDTJV.Columns.Add("SAPDim3", GetType(String))

            'Open the file in a stream reader.
            oSR = New StreamReader(CurrFileToUpload)

            Dim sText As String
            Dim sString(-1) As String
            ''  Dim sDelimiter As String() = {vbTab}
            'Dim sDelimiter As String() = {";"}
            Dim sDelimiter As String() = {","}

            While oSR.Peek <> -1
                sText = oSR.ReadLine()
                sString = sText.Split(sDelimiter, StringSplitOptions.None) ' "RemoveEmptyEntrie" I am also using the option to remove empty entries a
                'sString = sText.Split(" ")
                ' dtEntiry = p_oEntitesDetails.DefaultView.ToTable(True, sString(10))
                If sString.Length = "1" Then
                    If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Invalid File Format , preferable format is Txt {Tab} Delimiter  ", sFuncName)
                    Console.WriteLine("Invalid File Format , preferable format is Csv {,} Delimiter ")
                    sErrDesc = "Invalid File Format , preferable format is Csv {,} Delimiter  "
                    Exit While
                End If
                oDVGLMAP.RowFilter = "Code='" & sString(1) & "'"
                If oDVGLMAP.Count > 0 Then
                    sGLCode = oDVGLMAP.Item(0)(2)
                    sGLName = oDVGLMAP.Item(0)(3)
                Else
                    oDTMAP.Rows.Add(sString(1))
                    sGLCode = String.Empty
                    sGLName = String.Empty
                End If

                'oDVCC5.RowFilter = "PrcName='" & sString(5) & "'"
                'If oDVCC5.Count > 0 Then
                '    sSAPCC3 = oDVCC5.Item(0)(0)
                'Else
                '    oDTSAPDim3.Rows.Add(sString(5))
                '    'sCC3 = String.Empty
                '    sSAPCC3 = sString(5)
                'End If

                oDVCC3.RowFilter = "U_Column6='" & sString(5) & "'"
                If oDVCC3.Count > 0 Then
                    sCC3 = oDVCC3.Item(0)(0)
                Else
                    oDTDim3.Rows.Add(sString(5))
                    'sCC3 = String.Empty
                    sCC3 = sString(5)
                End If

                oDVCC4.RowFilter = "PrcName='" & sString(6) & "'"
                If oDVCC4.Count > 0 Then
                    sCC4 = oDVCC4.Item(0)(0)
                Else
                    oDTDim4.Rows.Add(sString(6))
                    'sCC4 = String.Empty
                    sCC4 = sString(6)
                End If

                'Commented on 11 Nov 2016 - Shibin
                'dDate = DateTime.ParseExact(sString(0), "dd-MMM-yy",
                '                                        CultureInfo.InvariantCulture)
                'sDate = Format(dDate, "yyyyMMdd")

                sDate = sString(0)

                'Dim monthName = sSplit(1)
                'Dim monthNumber As String = CStr(DateTime.ParseExact(monthName, "MMM", CultureInfo.CurrentCulture).Month)

                If Not String.IsNullOrEmpty(sString(3)) Then
                    dDebit = CDbl(sString(3))
                Else
                    dDebit = 0
                End If

                If Not String.IsNullOrEmpty(sString(4)) Then
                    dCredit = CDbl(sString(4))
                Else
                    dCredit = 0
                End If

                'oDTJV.Rows.Add(sDate, sString(1), sString(2), sGLCode, sGLName, dDebit, dCredit, sCC3, "")
                oDTJV.Rows.Add(sDate, sString(1), sString(2), sGLCode, sGLName, dDebit, dCredit, sCC3, sCC4)
            End While

            'Console.WriteLine("Del_schema() ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with SUCCESS ", sFuncName)
            'Del_schema(p_oCompDef.sInboxDir)

            dv = New DataView(oDTJV)
            Return dv

        Catch ex As Exception

            Console.WriteLine("Error occured while reading content of  " & ex.Message, sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Error occured while reading content of  " & ex.Message, sFuncName)
            Call WriteToLogFile(ex.Message, sFuncName)
            Return Nothing
        Finally
            oSR.Close()
            oSR = Nothing
        End Try

    End Function

    Public Function JournalVoucher_Posting(ByVal oDVJV As DataView, ByVal sfileName As String, ByRef sErrDesc As String) As Long
        ' **********************************************************************************
        '   Function    :   JournalVoucher_Posting()
        '   Purpose     :   This function will upload the data from  Dataview to Journal Voucher
        '   Parameters  :   ByVal oDVJV As DataView
        '                       ByVal sfileName As String
        '                          ByRef sErrDesc As String       
        '   Author      :   JOHN
        '   Date        :   NOV 2016 
        ' **********************************************************************************

        Dim sFuncName As String = String.Empty
        Dim ival As Integer
        Dim IsError As Boolean
        Dim iErr As Integer = 0
        Dim sErr As String = String.Empty
        Dim sJV As String = String.Empty
        Dim oJournalEntry As SAPbobsCOM.JournalVouchers = Nothing

        Try
            sFuncName = "JournalVoucher_Posting"
            Console.WriteLine("Starting Function ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function ", sFuncName)
            oJournalEntry = p_oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalVouchers)

            oJournalEntry.JournalEntries.ReferenceDate = DateTime.ParseExact(oDVJV.Item(0)("Pdate"), "yyyyMMdd", Nothing)
            '' oJournalEntry.JournalEntries.Memo = Left("Cost Allocation for the month of " & UCase(MonthName(Month(dDate))) & " - " & Year(dDate), 50)
            '' oJournalEntry.JournalEntries.Reference3 = sRef
            oJournalEntry.JournalEntries.Memo = sfileName

            For Each odr As DataRowView In oDVJV
                oJournalEntry.JournalEntries.Lines.AccountCode = odr("GLCode").ToString.Trim
                oJournalEntry.JournalEntries.Lines.Debit = CDbl(odr("Debit").ToString.Trim)
                oJournalEntry.JournalEntries.Lines.Credit = CDbl(odr("Credit").ToString.Trim)
                If Not String.IsNullOrEmpty(odr("Dim3").ToString.Trim) Then
                    oJournalEntry.JournalEntries.Lines.CostingCode3 = odr("Dim3").ToString.Trim 'OU
                End If
                If Not String.IsNullOrEmpty(odr("Dim4").ToString.Trim) Then
                    oJournalEntry.JournalEntries.Lines.CostingCode4 = odr("Dim4").ToString.Trim 'Project
                End If
                oJournalEntry.JournalEntries.Lines.BPLID = 2
                oJournalEntry.JournalEntries.Lines.UserFields.Fields.Item("U_AE_SuiteCode").Value = odr("SuiteCode").ToString.Trim
                oJournalEntry.JournalEntries.Lines.UserFields.Fields.Item("U_AE_SuiteName").Value = odr("SuiteName").ToString.Trim
                oJournalEntry.JournalEntries.Lines.Add()
            Next



            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Attempting to Add the Journal Voucher", sFuncName)
            ival = oJournalEntry.Add()

            If ival <> 0 Then
                IsError = True
                p_oCompany.GetLastError(iErr, sErr)
                Call WriteToLogFile("Completed with ERROR while adding the Journal Voucher ---" & sErr, sFuncName)
                Console.WriteLine("Completed with ERROR while adding the Journal Voucher", sFuncName)
                If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with ERROR while adding the Journal Voucher" & sErr, sFuncName)
                JournalVoucher_Posting = RTN_ERROR
                Throw New ArgumentException(sErr)
            End If

            Console.WriteLine("Completed with SUCCESS", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with SUCCESS while adding the Journal Voucher", sFuncName)
            p_oCompany.GetNewObjectCode(sJV)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Journal Voucher DocEntry  " & sJV, sFuncName)
            sErrDesc = String.Empty
            JournalVoucher_Posting = RTN_SUCCESS

        Catch ex As Exception
            sErrDesc = ex.Message

            Call WriteToLogFile(ex.Message, sFuncName)
            Console.WriteLine("Completed with ERROR while adding the Journal Voucher", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with ERROR while adding the Journal Voucher" & ex.Message, sFuncName)
            JournalVoucher_Posting = RTN_ERROR
            Exit Function
        End Try

    End Function

#End Region

#Region "ML Files"
    Public Function GetDataViewFromCSV_ML(ByVal CurrFileToUpload As String, ByVal Filename As String, ByRef oDTMAP As DataTable, ByRef sErrDesc As String) As DataView

        ' **********************************************************************************
        '   Function    :   GetDataViewFromCSV_ML()
        '   Purpose     :   This function will upload the data from CSV file to Dataview
        '   Parameters  :   ByRef CurrFileToUpload AS String 
        '                       ByVal Filename As String
        '                          ByRef oDTMAP As DataTable
        '                            ByRef sErrDesc As String
        '   Author      :   JOHN
        '   Date        :   NOV 2016 
        ' **********************************************************************************

        Dim dv As DataView

        Dim sFuncName As String = String.Empty
        sErrDesc = String.Empty
        Dim oSR As StreamReader

        Dim oDTJV As DataTable = Nothing
        Dim oDTML As DataTable = Nothing
        Dim oTBFBCA As DataTable = Nothing

        Dim oRset As SAPbobsCOM.Recordset = Nothing

        Dim dDate As Date
        Dim sDate As String = String.Empty


        Try
            sFuncName = "GetDataViewFromCSV_ML"
            Console.WriteLine("Starting Function", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function", sFuncName)
            oRset = p_oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oDTMAP.Clear()

            'The Datatable to Return
            oDTML = New DataTable()

            oDTML.Columns.Add("Date", GetType(String))
            oDTML.Columns.Add("Code", GetType(String))
            oDTML.Columns.Add("CodeName", GetType(String))
            oDTML.Columns.Add("Occupied", GetType(String))


            'Open the file in a stream reader.
            oSR = New StreamReader(CurrFileToUpload)

            Dim sText As String
            Dim sString(-1) As String
            ''  Dim sDelimiter As String() = {vbTab}
            Dim sDelimiter As String() = {","}

            While oSR.Peek <> -1
                sText = oSR.ReadLine()
                sString = sText.Split(sDelimiter, StringSplitOptions.None) ' "RemoveEmptyEntrie" I am also using the option to remove empty entries a
                'sString = sText.Split(" ")
                ' dtEntiry = p_oEntitesDetails.DefaultView.ToTable(True, sString(10))
                If sString.Length = "1" Then
                    If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Invalid File Format , preferable format is Txt {Tab} Delimiter  ", sFuncName)
                    Console.WriteLine("Invalid File Format , preferable format is Csv {,} Delimiter ")
                    sErrDesc = "Invalid File Format , preferable format is Csv {,} Delimiter  "
                    Exit While
                End If


                'dDate = DateTime.ParseExact(sString(0), "dd-MMM-yy",
                '                                        CultureInfo.InvariantCulture)
                'sDate = Format(dDate, "yyyyMMdd")
                sDate = sString(0)



                oDTML.Rows.Add(sDate, sString(1), sString(2), sString(3))
            End While

            'Console.WriteLine("Del_schema() ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with SUCCESS ", sFuncName)
            'Del_schema(p_oCompDef.sInboxDir)

            dv = New DataView(oDTML)
            Return dv

        Catch ex As Exception

            Console.WriteLine("Error occured while reading content of  " & ex.Message, sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Error occured while reading content of  " & ex.Message, sFuncName)
            Call WriteToLogFile(ex.Message, sFuncName)
            Return Nothing
        Finally
            oSR.Close()
            oSR = Nothing
        End Try

    End Function

    Public Function StatisticRoom(ByVal oDVSR As DataView, ByVal sfileName As String, ByRef sErrDesc As String) As Long

        ' *****************************************************************************************
        '   Function    :   StatisticRoom()
        '   Purpose     :   This function will upload the data from  Dataview to Occupied Room UDT
        '   Parameters  :   ByVal oDVJV As DataView
        '                       ByVal sfileName As String
        '                          ByRef sErrDesc As String       
        '   Author      :   JOHN
        '   Date        :   NOV 2016 
        ' *****************************************************************************************
        Dim sFuncName As String = String.Empty

        Dim iErr As Integer = 0
        Dim sErr As String = String.Empty
        Dim sJV As String = String.Empty


        Try
            sFuncName = "StatisticRoom"
            Console.WriteLine("Starting Function ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function ", sFuncName)

            Dim oGeneralService As SAPbobsCOM.GeneralService
            Dim oGeneralData As SAPbobsCOM.GeneralData
            Dim oChild As SAPbobsCOM.GeneralData
            Dim oChildren As SAPbobsCOM.GeneralDataCollection
            Dim oGeneralParams As SAPbobsCOM.GeneralDataParams
            Dim oCompService As SAPbobsCOM.CompanyService = p_oCompany.GetCompanyService()
            'p_oCompany.StartTransaction()
            oGeneralService = oCompService.GetGeneralService("RoomStatistic")
            oGeneralData = DirectCast(oGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData), SAPbobsCOM.GeneralData)

            oGeneralData.SetProperty("U_Date", DateTime.ParseExact(oDVSR.Item(0)("Date"), "yyyyMMdd", Nothing))
            oGeneralData.SetProperty("Remark", sfileName.ToString.Trim)
            For Each odr As DataRowView In oDVSR

                ' Adding data to Detail Line

                oChildren = oGeneralData.Child("TBLRM1")
                oChild = oChildren.Add()
                oChild.SetProperty("U_Code", odr("Code").ToString.Trim)
                oChild.SetProperty("U_Name", odr("CodeName").ToString.Trim)
                oChild.SetProperty("U_Occupied", odr("Occupied").ToString.Trim)

            Next
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Attempting to Add into Room Occupied UDT", sFuncName)
            oGeneralService.Add(oGeneralData)
            Console.WriteLine("Completed with SUCCESS while adding into Room Occupied UDT", sFuncName)

            sErrDesc = String.Empty
            StatisticRoom = RTN_SUCCESS

        Catch ex As Exception
            sErrDesc = ex.Message

            Call WriteToLogFile(ex.Message, sFuncName)
            Console.WriteLine("Completed with ERROR while adding into Room Occupied UDT", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with ERROR while adding into Room Occupied UDT" & ex.Message, sFuncName)
            StatisticRoom = RTN_ERROR
            Exit Function
        End Try

    End Function
#End Region

#Region "ST Files"
    Public Function GetDataViewFromCSV_ST(ByVal CurrFileToUpload As String, ByVal Filename As String, ByRef oDTMAP As DataTable, ByRef sErrDesc As String) As DataView


        ' **********************************************************************************
        '   Function    :   GetDataViewFromCSV_ST()
        '   Purpose     :   This function will upload the data from CSV file to Dataview
        '   Parameters  :   ByRef CurrFileToUpload AS String 
        '                       ByVal Filename As String
        '                          ByRef oDTMAP As DataTable
        '                            ByRef sErrDesc As String
        '   Author      :   JOHN
        '   Date        :   NOV 2016 
        ' **********************************************************************************

        Dim dv As DataView

        Dim sFuncName As String = String.Empty
        sErrDesc = String.Empty
        Dim oSR As StreamReader

        Dim oDTJV As DataTable = Nothing
        Dim oDTML As DataTable = Nothing
        Dim oTBFBCA As DataTable = Nothing

        Dim oRset As SAPbobsCOM.Recordset = Nothing
        Dim sDate As String = String.Empty


        Try
            sFuncName = "GetDataViewFromCSV_ST"
            Console.WriteLine("Starting Function", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function", sFuncName)
            oRset = p_oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oDTMAP.Clear()

            'The Datatable to Return
            oDTML = New DataTable()

            oDTML.Columns.Add("Date", GetType(String))
            oDTML.Columns.Add("Code", GetType(String))
            oDTML.Columns.Add("Cover", GetType(String))

            'Open the file in a stream reader.
            oSR = New StreamReader(CurrFileToUpload)

            Dim sText As String
            Dim sString(-1) As String
            ''  Dim sDelimiter As String() = {vbTab}
            Dim sDelimiter As String() = {","}

            While oSR.Peek <> -1
                sText = oSR.ReadLine()
                sString = sText.Split(sDelimiter, StringSplitOptions.None) ' "RemoveEmptyEntrie" I am also using the option to remove empty entries a
                'sString = sText.Split(" ")
                ' dtEntiry = p_oEntitesDetails.DefaultView.ToTable(True, sString(10))
                If sString.Length = "1" Then
                    If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Invalid File Format , preferable format is Txt {Tab} Delimiter  ", sFuncName)
                    Console.WriteLine("Invalid File Format , preferable format is Csv {,} Delimiter ")
                    sErrDesc = "Invalid File Format , preferable format is Csv {,} Delimiter  "
                    Exit While
                End If


                'dDate = DateTime.ParseExact(sString(0), "dd-MMM-yy",
                '                                        CultureInfo.InvariantCulture)
                'sDate = Format(dDate, "yyyyMMdd")
                sDate = sString(0)

                oDTML.Rows.Add(sDate, sString(1), sString(2))
            End While

            'Console.WriteLine("Del_schema() ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with SUCCESS ", sFuncName)
            dv = New DataView(oDTML)
            Return dv

        Catch ex As Exception

            Console.WriteLine("Error occured while reading content of  " & ex.Message, sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Error occured while reading content of  " & ex.Message, sFuncName)
            Call WriteToLogFile(ex.Message, sFuncName)
            Return Nothing
        Finally
            oSR.Close()
            oSR = Nothing
        End Try

    End Function

    Public Function FBCover(ByVal oDVSR As DataView, ByVal sfileName As String, ByRef sErrDesc As String) As Long
        ' *****************************************************************************************
        '   Function    :   FBCover()
        '   Purpose     :   This function will upload the data from  Dataview to F&B Actual UDT
        '   Parameters  :   ByVal oDVJV As DataView
        '                       ByVal sfileName As String
        '                          ByRef sErrDesc As String       
        '   Author      :   JOHN
        '   Date        :   NOV 2016 
        ' *****************************************************************************************
        Dim sFuncName As String = String.Empty
        Dim iErr As Integer = 0
        Dim sErr As String = String.Empty
        Dim sJV As String = String.Empty


        Try
            sFuncName = "FBCover"
            Console.WriteLine("Starting Function ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function ", sFuncName)

            Dim oGeneralService As SAPbobsCOM.GeneralService
            Dim oGeneralData As SAPbobsCOM.GeneralData
            Dim oChild As SAPbobsCOM.GeneralData
            Dim oChildren As SAPbobsCOM.GeneralDataCollection
            Dim oGeneralParams As SAPbobsCOM.GeneralDataParams
            Dim oCompService As SAPbobsCOM.CompanyService = p_oCompany.GetCompanyService()
            'p_oCompany.StartTransaction()
            'oGeneralService = oCompService.GetGeneralService("FBActual")
            'For Server
            oGeneralService = oCompService.GetGeneralService("CoverActual")

            oGeneralData = DirectCast(oGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData), SAPbobsCOM.GeneralData)

            oGeneralData.SetProperty("U_Date", DateTime.ParseExact(oDVSR.Item(0)("Date"), "yyyyMMdd", Nothing))
            oGeneralData.SetProperty("Remark", sfileName.ToString.Trim)

            For Each odr As DataRowView In oDVSR
                ' Adding data to Detail Line

                oChildren = oGeneralData.Child("TBFBCA1")
                oChild = oChildren.Add()
                oChild.SetProperty("U_Code", odr("Code").ToString.Trim)
                oChild.SetProperty("U_Cover", odr("Cover").ToString.Trim)

            Next
            oGeneralService.Add(oGeneralData)

            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Attempting to Add into F&B Actual UDT", sFuncName)
           
            Console.WriteLine("Completed with SUCCESS while adding into F&B Actual UDT", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with SUCCESS while adding into F&B Actual UDT", sFuncName)
            sErrDesc = String.Empty
            FBCover = RTN_SUCCESS

        Catch ex As Exception
            sErrDesc = ex.Message

            Call WriteToLogFile(ex.Message, sFuncName)
            Console.WriteLine("Completed with ERROR while adding into F&B Actual UDT", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with ERROR while adding into F&B Actual UDT" & ex.Message, sFuncName)
            FBCover = RTN_ERROR
            Exit Function
        End Try

    End Function

#End Region

#Region "MR Files"
    Public Function GetDataViewFromCSV_MR(ByVal CurrFileToUpload As String, ByVal Filename As String, ByRef oDTMAP As DataTable, ByRef sErrDesc As String) As DataView

        ' **********************************************************************************
        '   Function    :   GetDataViewFromCSV_MR()
        '   Purpose     :   This function will upload the data from CSV file to Dataview
        '   Parameters  :   ByRef CurrFileToUpload AS String 
        '                       ByVal Filename As String
        '                          ByRef oDTMAP As DataTable
        '                            ByRef sErrDesc As String
        '   Author      :   JOHN
        '   Date        :   NOV 2016 
        ' **********************************************************************************

        Dim dv As DataView

        Dim sFuncName As String = String.Empty
        sErrDesc = String.Empty
        Dim oSR As StreamReader

        Dim oDTJV As DataTable = Nothing
        Dim oDTML As DataTable = Nothing
        Dim oTBFBCA As DataTable = Nothing

        Dim oRset As SAPbobsCOM.Recordset = Nothing

        Dim sDate As String = String.Empty


        Try
            sFuncName = "GetDataViewFromCSV_MR"
            Console.WriteLine("Starting Function", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function", sFuncName)
            oRset = p_oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oDTMAP.Clear()

            'The Datatable to Return
            oDTML = New DataTable()

            oDTML.Columns.Add("Date", GetType(String))
            oDTML.Columns.Add("Code", GetType(String))
            oDTML.Columns.Add("CodeName", GetType(String))
            oDTML.Columns.Add("Amount", GetType(String))

            'Open the file in a stream reader.
            oSR = New StreamReader(CurrFileToUpload)

            Dim sText As String
            Dim sString(-1) As String
            ''  Dim sDelimiter As String() = {vbTab}
            Dim sDelimiter As String() = {","}

            While oSR.Peek <> -1
                sText = oSR.ReadLine()
                sString = sText.Split(sDelimiter, StringSplitOptions.None) ' "RemoveEmptyEntrie" I am also using the option to remove empty entries a
                'sString = sText.Split(" ")
                ' dtEntiry = p_oEntitesDetails.DefaultView.ToTable(True, sString(10))
                If sString.Length = "1" Then
                    If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Invalid File Format , preferable format is Txt {Tab} Delimiter  ", sFuncName)
                    Console.WriteLine("Invalid File Format , preferable format is Csv {,} Delimiter ")
                    sErrDesc = "Invalid File Format , preferable format is Csv {,} Delimiter  "
                    Exit While
                End If


                'dDate = DateTime.ParseExact(sString(0), "dd-MMM-yy",
                '                                        CultureInfo.InvariantCulture)
                'sDate = Format(dDate, "yyyyMMdd")
                sDate = sString(0)

                oDTML.Rows.Add(sDate, sString(1), sString(2), sString(3))
            End While

            'Console.WriteLine("Del_schema() ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with SUCCESS ", sFuncName)
            'Del_schema(p_oCompDef.sInboxDir)

            dv = New DataView(oDTML)
            Return dv

        Catch ex As Exception

            Console.WriteLine("Error occured while reading content of  " & ex.Message, sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Error occured while reading content of  " & ex.Message, sFuncName)
            Call WriteToLogFile(ex.Message, sFuncName)
            Return Nothing
        Finally
            oSR.Close()
            oSR = Nothing
        End Try

    End Function

    Public Function RoomRevenue(ByVal oDVSR As DataView, ByVal sfileName As String, ByRef sErrDesc As String) As Long
        ' *****************************************************************************************
        '   Function    :   RoomRevenue()
        '   Purpose     :   This function will upload the data from  Dataview to RoomRevenue UDT
        '   Parameters  :   ByVal oDVJV As DataView
        '                       ByVal sfileName As String
        '                          ByRef sErrDesc As String       
        '   Author      :   JOHN
        '   Date        :   NOV 2016 
        ' *****************************************************************************************
        Dim sFuncName As String = String.Empty
        Dim iErr As Integer = 0
        Dim sErr As String = String.Empty
        Dim sJV As String = String.Empty


        Try
            sFuncName = "RoomRevenue"
            Console.WriteLine("Starting Function ", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function ", sFuncName)

            Dim oGeneralService As SAPbobsCOM.GeneralService
            Dim oGeneralData As SAPbobsCOM.GeneralData
            Dim oChild As SAPbobsCOM.GeneralData
            Dim oChildren As SAPbobsCOM.GeneralDataCollection
            Dim oGeneralParams As SAPbobsCOM.GeneralDataParams
            Dim oCompService As SAPbobsCOM.CompanyService = p_oCompany.GetCompanyService()
            'p_oCompany.StartTransaction()
            oGeneralService = oCompService.GetGeneralService("Room")
            oGeneralData = DirectCast(oGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData), SAPbobsCOM.GeneralData)

            oGeneralData.SetProperty("U_Date", DateTime.ParseExact(oDVSR.Item(0)("Date"), "yyyyMMdd", Nothing))
            oGeneralData.SetProperty("Remark", sfileName.ToString.Trim)
            For Each odr As DataRowView In oDVSR

                ' Adding data to Detail Line

                oChildren = oGeneralData.Child("TBLRMV1")
                oChild = oChildren.Add()
                oChild.SetProperty("U_Code", odr("Code").ToString.Trim)
                oChild.SetProperty("U_CodeName", odr("CodeName").ToString.Trim)
                oChild.SetProperty("U_Amount", odr("Amount").ToString.Trim)

            Next
            oGeneralService.Add(oGeneralData)

            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Attempting to Add into RoomRevenue UDT", sFuncName)

            Console.WriteLine("Completed with SUCCESS while adding into Room Revenue By Market Segment UDT", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with SUCCESS while adding into Room Revenue By Market Segment UDT", sFuncName)
            sErrDesc = String.Empty
            RoomRevenue = RTN_SUCCESS

        Catch ex As Exception
            sErrDesc = ex.Message

            Call WriteToLogFile(ex.Message, sFuncName)
            Console.WriteLine("Completed with ERROR while adding into Room Revenue By Market Segment UDT", sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with ERROR while adding into Room Revenue By Market Segment UDT" & ex.Message, sFuncName)
            RoomRevenue = RTN_ERROR
            Exit Function
        End Try

    End Function
#End Region

#Region "MoveFile"
    Public Sub MoveFile(targetPath As [String], sourcePath As String, fileName As [String])
        '  ****************************************************************************************************
        '    Function    :   MoveFile()
        '    Purpose     :   This function is to Move file to success/failure folder
        '                
        '    Parameters  :  
        '                    string targetPath
        '                       string sourcePath        
        '                              String fileName
        '                           
        '    Author      :   SHIBIN
        '    Date        :   Nov 2016
        '  *****************************************************************************************************

        Dim sFuncName As String = String.Empty
        'Dim fileNameParts As String() = fileName.ToString().Split("."c)
        'Dim partialfile As String = fileNameParts(1) + "."c + fileNameParts(2) + "."c + fileNameParts(3)
        Dim hdDirectoryInWhichToSearch As New DirectoryInfo(sourcePath)
        Dim filesInDir As FileInfo() = hdDirectoryInWhichToSearch.GetFiles(fileName)
        sFuncName = "MoveFile"
        Try
            For Each foundFile As FileInfo In filesInDir
                Dim fullName As String = foundFile.Name
                'Console.WriteLine(fullName);
                Dim sourceFile As String = System.IO.Path.Combine(sourcePath, fullName)
                Dim destFile As String = System.IO.Path.Combine(targetPath, fullName)
                Dim sFileName As String = fullName.Substring(0, (fullName.Length() - 4))
                'Dim sDate As String = Now.ToString("yyyyMMddhhmmsstt")

                Dim RenameCurrFile As String = sFileName & "_" & Now.ToString("yyyyMMddHHmmsstt") & ".csv"
                If True Then
                    System.IO.Directory.CreateDirectory(targetPath)
                End If
                If File.Exists(destFile) Then

                    If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("If file exists in Success/Failure folder - deleting.... ", sFuncName)
                    File.Delete(destFile)

                End If

                foundFile.MoveTo(targetPath & "\" & RenameCurrFile)


                If (p_iDebugMode = DEBUG_ON) Then Call WriteToLogFile_Debug("Moving the file to Success/Failure folder....", sFuncName)
                'System.IO.File.Move(sourceFile, destFile)

            Next
        Catch ex As Exception
            WriteToLogFile(ex.Message, sFuncName)
            If (p_iDebugMode = DEBUG_ON) Then Call WriteToLogFile_Debug("Unable to Move the file after uploading to Success/Failure folder....", sFuncName)

            'SBO_Application.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try

    End Sub
#End Region

#Region "Delete File"
    Public Sub DeleteFile(sUploadFilePath As [String], sZippedFilePath As String)
        '  ****************************************************************************************************
        '    Function    :   DeleteFile()
        '    Purpose     :   This function is to Delete  file after getting upload
        '                
        '    Parameters  :  
        '                    string sUploadFilePath
        '                       string sZippedFilePath
        '                           
        '    Author      :   SHIBIN
        '    Date        :   MAY 2016
        '  *****************************************************************************************************
        Dim sFuncName As String = String.Empty
        sFuncName = "DeleteFile"

        If System.IO.File.Exists(sUploadFilePath) Then
            If (p_iDebugMode = DEBUG_ON) Then
                Call WriteToLogFile_Debug("Deleteing file .....", sFuncName)
            End If
            Try
                System.IO.File.Delete(sUploadFilePath)
            Catch ex As System.IO.IOException
                'Console.WriteLine(ex.Message);
                WriteToLogFile(ex.Message, sFuncName)
                If (p_iDebugMode = DEBUG_ON) Then
                    Call WriteToLogFile_Debug("Unable to Delete the file ....", sFuncName)
                End If
                'SBO_Application.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)

                Return
            End Try
        End If


    End Sub
#End Region
    'ByRef oDTMAP As DataTable
#Region "Mail"
    Public Function SendEmailNotification(ByVal oDTBody As DataTable) As Long
        ' *****************************************************************************************
        '   Function    :   SendEmailNotification()
        '   Purpose     :   This function will send a email notification
        '   Parameters  :   ByVal oDTBody As DataTable    
        '   Author      :   JOHN
        '   Date        :   NOV 2016
        ' *****************************************************************************************
        Dim sFuncName As String = String.Empty
        Dim sErrDesc As String = String.Empty
        Dim oSmtpServer As New SmtpClient()
        Dim oMail As New MailMessage
        Dim p_SyncDateTime As String = String.Empty

        Try
            sFuncName = "SendEmailNotification()"
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Starting Function...", sFuncName)

            'Status,ErrorDescription
            Dim sBody As String = String.Empty ''= "<table>"
            Dim str As New StringBuilder
            Dim sError As String = "Error"
            Dim sDate As String = Date.Now.ToString("dd-MM-yyyy HH:mm:ss")
           
            sBody = sBody & "<div align=left style='font-size:10.0pt;font-family:Arial'>"
            Dim contains As Boolean = oDTBody.AsEnumerable().Any(Function(row) sError = row.Field(Of [String])("Status"))

            If contains = True Then
                sBody = sBody & "Dear Valued Customer,<br /><br />"                
                sBody = sBody & "Suite8 Interface program had encountered the problem based on the following information.  <br /><br />"
                sBody = sBody & "Please contact your system administrator or Technical consultant to assist you for further information if any failure.  <br /><br />"
            Else
                sBody = sBody & "Dear Valued Customer,<br /><br />"
                sBody = sBody & "Suite8 Interface program had Successfully uploaded the files into SAP.  <br /><br />"
                sBody = sBody & "Please find the below list of files.   <br />"
                'sBody = sBody & "Date: " & sDate & "<br />"
            End If



            'sBody = sBody & "Dear Valued Customer,<br /><br />"
            'sBody = sBody & "SAP Interface status updates on the Journal Voucher post/Statistic Tables over for your kind reference.  <br /><br />"
            'sBody = sBody & "Suite8 Interface program had encountered the problem based on the following information.  <br />"
            'sBody = sBody & "Please contact your system administrator or Technical consultant to assist you for further information if any failure.  <br /><br />"
            sBody = sBody & "<br /><br />"
            sBody += "<html><head><style>table,th, td {"
            sBody += "border: 1px solid green; border-collapse: collapse;}</style></head>"

            'sBody += "<body style='font-size:12px;font-family:Arial;'>"
            'sBody += "<table style='width:100%'>" style='border-top:5px solid black;
            sBody += "<body style='font-size:12px;font-family:Arial;'><table width='600px' align='left' border='0' cellpadding='3' cellspacing='0''>"
            sBody += "<tr><th>File Name</th><th>Status</th><th>Comments</th></tr> <tr>"

            For Each row As DataRow In oDTBody.Rows

                sBody += "<tr><td>" & row("FileName") & "</td><td>" & row("Status") & "</td><td>" & row("ErrorDescription") & "</td></tr>"
                'sBody += "<tr><th>S.No</th><th>File Name</th><th>Status</th><th>Error Description</th></tr> <tr><td>" & r("SNo") & "</td><td>" & r("FileName") & "</td><td>" & r("Status") & "</td><td>" & r("ErrorDescription") & "</td></tr></table>"

            Next

            sBody = sBody & "</table><br /><br /><br /><br /></body></html>"
            sBody = sBody & "<br /><br />"
            sBody = sBody & "<br /><br />"
            sBody = sBody & "<br /><br />"
            sBody = sBody & "Note: This email message is computer generated and it will be used internal purpose usage only."

            oSmtpServer.Credentials = New Net.NetworkCredential(p_oCompDef.sSMTPUser, p_oCompDef.sSMTPPassword)
            oSmtpServer.Port = p_oCompDef.sSMTPPort '587
            oSmtpServer.Host = p_oCompDef.sSMTPServer '"smtp.gmail.com"
            If p_oCompDef.sSSL = "ON" Then
                oSmtpServer.EnableSsl = True
            Else
                oSmtpServer.EnableSsl = False
            End If
            '
            oMail.From = New MailAddress(p_oCompDef.sEmailFrom) '("sapb1.abeoelectra@gmail.com")
            oMail.To.Add(p_oCompDef.sToEmailID)

            oMail.Subject = "Suite8 Interface program"
            oMail.Body = sBody
            oMail.IsBodyHtml = True
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Email Notification Sending to " & p_oCompDef.sToEmailID, sFuncName)
            oSmtpServer.Send(oMail)
            oMail.Dispose()


            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with SUCCESS", sFuncName)
            SendEmailNotification = RTN_SUCCESS
        Catch ex As Exception
            sErrDesc = ex.Message
            '' oMail.Dispose()
            WriteToLogFile(ex.Message, sFuncName)
            If p_iDebugMode = DEBUG_ON Then Call WriteToLogFile_Debug("Completed with ERROR", sFuncName)
            'Console.WriteLine("Completed with Error " & sFuncName)
            SendEmailNotification = RTN_ERROR
        Finally
            oMail.Dispose()

        End Try

    End Function
#End Region

End Module
