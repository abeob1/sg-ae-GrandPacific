Option Explicit On

Imports System.IO

Module modLog


    '***************************************
    'Name       :   modLog
    'Descrption :   Contains function for log errors and Application related information
    'Author     :   SHIBIN
    'Created    :   Sep 2016
    '***************************************

    Private Const MAXFILESIZE_IN_MB As Int16 = 5 '(2 MB)
    Private Const LOG_FILE_ERROR As String = "ErrorLog"
    Private Const LOG_FILE_ERROR_ARCH As String = "ErrorLog_"
    Private Const LOG_FILE_DEBUG As String = "DebugLog"
    Private Const LOG_FILE_DEBUG_ARCH As String = "DebugLog_"
    Private Const FILE_SIZE_CHECK_ENABLE As Int16 = 1
    Private Const FILE_SIZE_CHECK_DISABLE As Int16 = 0

    Public Function WriteToLogFile(ByVal strErrText As String, ByVal strSourceName As String, Optional ByVal intCheckFileForDelete As Int16 = 1) As Long

        ' **********************************************************************************
        '   Function   :    WriteToLogFile()
        '   Purpose    :    This function checks if given input file name exists or not
        '
        '   Parameters :    ByVal strErrText As String
        '                       strErrText = Text to be written to the log
        '                   ByVal intLogType As Integer
        '                       intLogType = Log type (1 - Log ; 2 - Error ; 0 - None)
        '                   ByVal strSourceName As String
        '                       strSourceName = Function name calling this function
        '                   Optional ByVal intCheckFileForDelete As Integer
        '                       intCheckFileForDelete = Flag to indicate if file size need to be checked before logging (0 - No check ; 1 - Check)
        '   Return     :    0 - FAILURE
        '                   1 - SUCCESS
        '   Author     :    SHIBIN
        '   Date       :    Sep 2016
        ' **********************************************************************************

        Dim oStreamWriter As StreamWriter = Nothing
        Dim strFileName As String = String.Empty
        Dim strArchFileName As String = String.Empty
        Dim strTempString As String = String.Empty
        Dim lngFileSizeInMB As Double

        Try
            strTempString = Space(IIf(Len(strSourceName) > 30, 0, 30 - Len(strSourceName)))
            strSourceName = strTempString & strSourceName
            strErrText = "[" & Format(Now, "MM/dd/yyyy HH:mm:ss") & "]" & "[" & strSourceName & "] " & strErrText

            'strFileName = System.Windows.Forms.Application.StartupPath & "\" & "LOG_FILE_ERROR" & ".log"
            strFileName = p_oCompDef.sFilepath & "\" & "LOG_FILE_ERROR" & ".log"
            'strArchFileName = System.Windows.Forms.Application.StartupPath & "\" & "LOG_FILE_ERROR_ARCH" & Format(Now(), "yyMMddHHMMss") & ".log"
            strArchFileName = p_oCompDef.sFilepath & "\" & "LOG_FILE_ERROR_ARCH" & Format(Now(), "yyMMddHHMMss") & ".log"

            'strFileName = p_oCompDef.sFilepath & "\" & LOG_FILE_ERROR & ".log"
            'strArchFileName = p_oCompDef.sFilepath & "\" & LOG_FILE_ERROR_ARCH & Format(Now(), "yyMMddHHMMss") & ".log"

            If intCheckFileForDelete = FILE_SIZE_CHECK_ENABLE Then
                If File.Exists(strFileName) Then
                    lngFileSizeInMB = (FileLen(strFileName) / 1024) / 1024
                    If lngFileSizeInMB >= MAXFILESIZE_IN_MB Then                        
                        If p_iDeleteDebugLog = 1 Then
                            For Each sFileName As String In Directory.GetFiles(System.IO.Directory.GetCurrentDirectory(), LOG_FILE_DEBUG_ARCH & "*")
                                File.Delete(sFileName)
                            Next
                        End If
                        File.Move(strFileName, strArchFileName)
                    End If
                End If
            End If
            oStreamWriter = File.AppendText(strFileName)
            oStreamWriter.WriteLine(strErrText)
            WriteToLogFile = RTN_SUCCESS
        Catch exc As Exception
            WriteToLogFile = RTN_ERROR
        Finally
            If Not IsNothing(oStreamWriter) Then
                oStreamWriter.Flush()
                oStreamWriter.Close()
                oStreamWriter = Nothing
            End If
        End Try

    End Function

    Public Function WriteToLogFile_Debug(ByVal strErrText As String, ByVal strSourceName As String, Optional ByVal intCheckFileForDelete As Int16 = 1) As Long
        ' **********************************************************************************
        '   Function   :    WriteToLogFile_Debug()
        '   Purpose    :    This function checks if given input file name exists or not
        '
        '   Parameters :    ByVal strErrText As String
        '                       strErrText = Text to be written to the log
        '                   ByVal intLogType As Integer
        '                       intLogType = Log type (1 - Log ; 2 - Error ; 0 - None)
        '                   ByVal strSourceName As String
        '                       strSourceName = Function name calling this function
        '                   Optional ByVal intCheckFileForDelete As Integer
        '                       intCheckFileForDelete = Flag to indicate if file size need to be checked before logging (0 - No check ; 1 - Check)
        '   Return     :    0 - FAILURE
        '                   1 - SUCCESS
        '   Author     :    SHIBIN
        '   Date       :    Sep 2016
        '   Changes    : 
        '                   
        ' **********************************************************************************

        Dim oStreamWriter As StreamWriter = Nothing
        Dim strFileName As String = String.Empty
        Dim strArchFileName As String = String.Empty
        Dim strTempString As String = String.Empty
        Dim lngFileSizeInMB As Double
        Dim iFileCount As Integer = 0

        Try
            strTempString = Space(IIf(Len(strSourceName) > 30, 0, 30 - Len(strSourceName)))
            strSourceName = strTempString & strSourceName
            strErrText = "[" & Format(Now, "MM/dd/yyyy HH:mm:ss") & "]" & "[" & strSourceName & "] " & strErrText

            'strFileName = System.Windows.Forms.Application.StartupPath & "\" & "LOG_FILE_DEBUG" & ".log"
            strFileName = p_oCompDef.sFilepath & "\" & "LOG_FILE_DEBUG" & ".log"
            'strArchFileName = System.Windows.Forms.Application.StartupPath & "\" & "LOG_FILE_DEBUG_ARCH" & Format(Now(), "yyMMddHHMMss") & ".log"
            strArchFileName = p_oCompDef.sFilepath & "\" & "LOG_FILE_DEBUG_ARCH" & Format(Now(), "yyMMddHHMMss") & ".log"
            'strFileName = p_oCompDef.sFilepath & "\" & LOG_FILE_DEBUG & ".log"
            'strArchFileName = p_oCompDef.sFilepath & "\" & LOG_FILE_DEBUG_ARCH & Format(Now(), "yyMMddHHMMss") & ".log"


            
            If intCheckFileForDelete = FILE_SIZE_CHECK_ENABLE Then
                If File.Exists(strFileName) Then
                    lngFileSizeInMB = (FileLen(strFileName) / 1024) / 1024
                    If lngFileSizeInMB >= MAXFILESIZE_IN_MB Then                       
                        If p_iDeleteDebugLog = 1 Then
                            For Each sFileName As String In Directory.GetFiles(System.IO.Directory.GetCurrentDirectory(), LOG_FILE_DEBUG_ARCH & "*")
                                File.Delete(sFileName)
                            Next
                        End If
                        File.Move(strFileName, strArchFileName)
                    End If
                End If
            End If
            oStreamWriter = File.AppendText(strFileName)
            oStreamWriter.WriteLine(strErrText)
            WriteToLogFile_Debug = RTN_SUCCESS
        Catch exc As Exception
            WriteToLogFile_Debug = RTN_ERROR
        Finally
            If Not IsNothing(oStreamWriter) Then
                oStreamWriter.Flush()
                oStreamWriter.Close()
                oStreamWriter = Nothing
            End If
        End Try

    End Function

    Public Function WriteToLogFile_Sync(ByVal strErrText As String, Optional ByVal intCheckFileForDelete As Int16 = 1) As Long

        ' **********************************************************************************
        '   Function   :    WriteToLogFile_Debug()
        '   Purpose    :    This function checks if given input file name exists or not
        '
        '   Parameters :    ByVal strErrText As String
        '                       strErrText = Text to be written to the log
        '                   ByVal intLogType As Integer
        '                       intLogType = Log type (1 - Log ; 2 - Error ; 0 - None)
        '                   ByVal strSourceName As String
        '                       strSourceName = Function name calling this function
        '                   Optional ByVal intCheckFileForDelete As Integer
        '                       intCheckFileForDelete = Flag to indicate if file size need to be checked before logging (0 - No check ; 1 - Check)
        '   Return     :    0 - FAILURE
        '                   1 - SUCCESS
        '   Author     :    SRI
        '   Date       :    29 April 2013
        '   Changes    : 
        ' **********************************************************************************

        Dim oStreamWriter As StreamWriter = Nothing
        Dim strFileName As String = String.Empty
        Dim strArchFileName As String = String.Empty
        Dim strTempString As String = String.Empty
        Dim lngFileSizeInMB As Double
        Dim iFileCount As Integer = 0

        Try
            ''  strTempString = Space(30 - Len(strSourceName))
            ''  strSourceName = strTempString & strSourceName
            strErrText = "[" & strErrText & "] "

            'strFileName = System.Windows.Forms.Application.StartupPath & "\" & "SYNC" & ".log"
            strFileName = p_oCompDef.sFilepath & "\" & "SYNC" & ".log"
            'strArchFileName = System.Windows.Forms.Application.StartupPath & "\" & "SYNC_ARCH" & Format(Now(), "yyMMddHHMMss") & ".log"
            strArchFileName = p_oCompDef.sFilepath & "\" & "SYNC_ARCH" & Format(Now(), "yyMMddHHMMss") & ".log"

            'strFileName = p_oCompDef.sFilepath & "\" & LOG_FILE_DEBUG & ".log"
            'strArchFileName = p_oCompDef.sFilepath & "\" & LOG_FILE_DEBUG_ARCH & Format(Now(), "yyMMddHHMMss") & ".log"

            If intCheckFileForDelete = FILE_SIZE_CHECK_ENABLE Then
                If File.Exists(strFileName) Then
                    lngFileSizeInMB = (FileLen(strFileName) / 1024) / 1024
                    If lngFileSizeInMB >= MAXFILESIZE_IN_MB Then
                        'If intCheckDeleteDebugLog=1 then remove all debug_log file
                        If p_iDeleteDebugLog = 1 Then
                            For Each sFileName As String In Directory.GetFiles(System.Windows.Forms.Application.StartupPath, LOG_FILE_DEBUG_ARCH & "*")
                                File.Delete(sFileName)
                            Next
                        End If
                        File.Move(strFileName, strArchFileName)
                    End If
                End If
            End If
            oStreamWriter = File.AppendText(strFileName)
            oStreamWriter.WriteLine(strErrText)
            WriteToLogFile_Sync = RTN_SUCCESS
        Catch exc As Exception
            WriteToLogFile_Sync = RTN_ERROR
        Finally
            If Not IsNothing(oStreamWriter) Then
                oStreamWriter.Flush()
                oStreamWriter.Close()
                oStreamWriter = Nothing
            End If
        End Try

    End Function

    Public Sub Write_Validation(ByVal oDTGL As DataTable, ByVal oDTDim3 As DataTable, ByVal oDTDim4 As DataTable, ByVal sFileName As String)
        Try

            Dim sPath As String = p_oCompDef.sFilepath & "\"
            Dim sFName As String = "JV_Validation.txt"

            Dim sw As StreamWriter = New StreamWriter(sPath & sFName)
            ' Add some text to the file.

            sw.WriteLine("Validation Error! ")
            sw.WriteLine(" File Name - " & sFileName)
            sw.WriteLine("Sync Date  - " & Now.ToLongDateString)
            sw.WriteLine("")
            If oDTGL.Rows.Count > 0 Then
                For Each odr As DataRow In oDTGL.Rows
                    sw.WriteLine("Code " & odr(0) & " is not defined in the mapping table")
                Next
            End If
            sw.WriteLine("")
            If oDTDim3.Rows.Count > 0 Then
                For Each odr As DataRow In oDTDim3.Rows
                    sw.WriteLine("Code " & odr(0) & " is not defined in the Cost Center (Dimension3)")
                Next
            End If
            sw.WriteLine("")
            If oDTDim4.Rows.Count > 0 Then
                For Each odr As DataRow In oDTDim4.Rows
                    sw.WriteLine("Code " & odr(0) & " is not defined in the Cost Center (Dimension4)")
                Next
            End If
            sw.WriteLine(" ")
            sw.WriteLine("================================================================================================")
            sw.WriteLine("Please check ")
            sw.Close()
            Process.Start(sPath & sFName)

        Catch ex As Exception

        End Try

    End Sub

End Module
