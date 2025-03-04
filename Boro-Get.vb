Imports System.IO.Compression
Imports Microsoft.Win32
Namespace Boro_Get

    Module Handler
        Function BORO_GET_ADMIN(ByVal command As String) As String
            Try
                AddToLog("BOROGET", "Processing: " & command, False)
                Dim boroGETcommand As String = command
                If boroGETcommand.StartsWith("boro-get") Then
                    boroGETcommand = boroGETcommand.Remove(0, 9)
                End If
                boroGETcommand = boroGETcommand.TrimStart()
                boroGETcommand = boroGETcommand.TrimEnd()
                Select Case boroGETcommand
                    Case "install" 'boro-get install
                        Return InstallBOROGET()
                    Case "uninstall" 'boro-get uninstall
                        Return UninstallBOROGET()
                    Case "reinstall" 'boro-get reinstall
                        Return ReinstallBOROGET()
                    Case "status" 'boro-get status
                        If isBoroGetInstalled() Then
                            Dim check_boroget As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                            Return "Installed! (" & check_boroget.GetValue("Name") & " " & check_boroget.GetValue("Version") & ")"
                        Else
                            Return "No installed"
                        End If
                    Case "set" 'boro-get set llave|valors
                        Dim args() As String = boroGETcommand.Split("|")
                        Return SetBOROGET(args(1), args(2))
                    Case "reset" 'boro-get reset
                        If My.Computer.FileSystem.DirectoryExists(DIRInstallFolder) Then
                            My.Computer.FileSystem.DeleteDirectory(DIRInstallFolder, FileIO.DeleteDirectoryOption.DeleteAllContents)
                        End If
                        Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
                        regKey.DeleteSubKeyTree("boro-get")
                        Return "Local repository has been cleared!"
                    Case Else 'boro-get <component>, <run_or_not>, <params>...
                        If isBoroGetInstalled() Then
                            'paquete
                            Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                            Process.Start(regKey.GetValue("boro-get"), boroGETcommand)
                            Return "Processing (" & boroGETcommand & ")"
                        Else
                            Return "boro-get is not installed."
                        End If
                End Select
            Catch ex As Exception
                AddToLog("BORO_GET_ADMIN@BOROGET", "Error: " & ex.Message, True)
                Return "Error processing 'boro-get' command."
            End Try
        End Function
    End Module

    Module Manager
        Public DIRInstallFolder As String = DIRCommons & "\boro-get"
        Public BoroGet_zipPackage As String = DIRInstallFolder & "\boro-get.zip"
        Public BoroGet_configFile As String = DIRInstallFolder & "\config.ini"

        Function isBoroGetInstalled()
            Try
                Dim check_boroget As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                If check_boroget Is Nothing Then
                    Return False
                Else
                    If check_boroget.GetValue("boro-get") = Nothing Then
                        Return False
                    Else
                        Return True
                    End If
                End If
            Catch ex As Exception
                AddToLog("isBoroGetInstalled@BOROGET", "Error: " & ex.Message, True)
                Return False
            End Try
        End Function
        Function InstallBOROGET() As String
            Try
                AddToLog("BOROGET", "Installing boro-get...", False)
                If Not My.Computer.FileSystem.DirectoryExists(DIRInstallFolder) Then
                    My.Computer.FileSystem.CreateDirectory(DIRInstallFolder)
                End If
                If My.Computer.FileSystem.FileExists(BoroGet_zipPackage) Then
                    My.Computer.FileSystem.DeleteFile(BoroGet_zipPackage)
                End If
                If My.Computer.FileSystem.FileExists(BoroGet_configFile) Then
                    My.Computer.FileSystem.DeleteFile(BoroGet_configFile)
                End If
                'Descargar desde el servidor
                AddToLog("BOROGET", "Downloading boro-get configuration file...", False)
                My.Computer.Network.DownloadFile(GetIniValue("boro-get", "Configuration", DIRCommons & "\Globals.ini"), BoroGet_configFile)
                AddToLog("BOROGET", "Downloading boro-get...", False)
                My.Computer.Network.DownloadFile(GetIniValue("CONFIG", "Binaries", BoroGet_configFile), BoroGet_zipPackage)
                'Instalar
                AddToLog("BOROGET", "Extracting boro-get...", False)
                ZipFile.ExtractToDirectory(BoroGet_zipPackage, DIRInstallFolder)
                'Registra
                AddToLog("BOROGET", "Registering boro-get...", False)
                Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                If regKey Is Nothing Then
                    Registry.CurrentUser.CreateSubKey("SOFTWARE\\Borocito\\boro-get")
                    regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                End If
                regKey.SetValue("boro-get", DIRInstallFolder & "\boro-get.exe")
                regKey.SetValue("Name", GetIniValue("ASSEMBLY", "Name", BoroGet_configFile))
                regKey.SetValue("Version", GetIniValue("ASSEMBLY", "Version", BoroGet_configFile))
                regKey.SetValue("Author", GetIniValue("ASSEMBLY", "Author", BoroGet_configFile))
                regKey.SetValue("Website", GetIniValue("ASSEMBLY", "Web", BoroGet_configFile))
                regKey.SetValue("Repository", GetIniValue("CONFIG", "Repositories", BoroGet_configFile))
                Return "boro-get has been installed!"
            Catch ex As Exception
                AddToLog("Install@BOROGET", "Error: " & ex.Message, True)
                Return "Error installing boro-get."
            End Try
        End Function
        Function UninstallBOROGET() As String
            Try
                AddToLog("BOROGET", "Uninstalling boro-get...", False)
                'Eliminar directorio
                If My.Computer.FileSystem.DirectoryExists(DIRInstallFolder) Then
                    My.Computer.FileSystem.DeleteDirectory(DIRInstallFolder, FileIO.DeleteDirectoryOption.DeleteAllContents)
                End If
                'Eliminar registros
                Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito", True)
                regKey.DeleteSubKeyTree("boro-get")
                Return "boro-get has been uninstalled!"
            Catch ex As Exception
                AddToLog("Uninstall@BOROGET", "Error: " & ex.Message, True)
                Return "Error uninstalling boro-get."
            End Try
        End Function
        Function ReinstallBOROGET() As String
            Try
                AddToLog("BOROGET", "Reinstalling boro-get...", False)
                Dim regKey As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                Dim boroget_FilePath As String = regKey.GetValue("boro-get")
                'Eliminar los binarios. (NO COMPONENTES)
                If My.Computer.FileSystem.FileExists(boroget_FilePath) Then
                    My.Computer.FileSystem.DeleteFile(boroget_FilePath)
                End If
                'Volver a instalar
                InstallBOROGET()
                Return "boro-get has been reinstalled!"
            Catch ex As Exception
                AddToLog("Uninstall@BOROGET", "Error: " & ex.Message, True)
                Return "Error uninstalling boro-get."
            End Try
        End Function
        Function SetBOROGET(ByVal regKey As String, ByVal regValue As String) As String
            Try
                AddToLog("BOROGET", "Seting boro-get value...", False)
                Dim RegeditBoroGet As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                If RegeditBoroGet Is Nothing Then
                    Registry.CurrentUser.CreateSubKey("SOFTWARE\\Borocito\\boro-get")
                    RegeditBoroGet = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Borocito\\boro-get", True)
                End If
                RegeditBoroGet.SetValue(regKey, regValue)
                Return "Key: " & regKey & " Value: " & regValue & " has been setted!"
            Catch ex As Exception
                AddToLog("Set@BOROGET", "Error: " & ex.Message, True)
                Return "Error setting registry."
            End Try
        End Function
    End Module

End Namespace
