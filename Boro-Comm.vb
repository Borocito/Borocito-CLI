Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Namespace Boro_Comm
    'BORO-COMM es el nuevo sistema de comunicacion Servidor-Cliente-Servidor
    'Utiliza el (proximo) complemento Boro-Comm para realizar la tarea de comunicar con algun proveedor de comandos
    'Como:
    '   Firebase by Google
    '       Realtime Database
    '   TCP/IP (for Local network or Wordwide)
    '   IDFTP (Actual system)
    '   Another (Custom, developer by You or others...)

    Module Connector
        Dim ServidorTCP As TCPServer

        ' Evento de botón para iniciar el servidor
        Sub StartServer()
            Try
                ServidorTCP = New TCPServer
                ServidorTCP.StartServer()
                AddHandler ServidorTCP.MessageReceived, AddressOf MensajeRecibido
            Catch ex As Exception
                AddToLog("StartServer@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub
        Sub StopServer()
            Try
                ServidorTCP.StopServer()
            Catch ex As Exception
                AddToLog("StopServer@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub
        Function MensajeRecibido(sender As Object, e As String) As String
            Try
                Return SendMesssage(Network.CommandManager.CommandManager(e))
            Catch ex As Exception
                Return AddToLog("MensajeRecibido@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Function

        ' Método para enviar un mensaje al cliente
        Function SendMesssage(message As String) As String
            Try
                If ServidorTCP IsNot Nothing AndAlso message IsNot Nothing Then
                    ServidorTCP.SendMessageToClient(message)
                    Console.WriteLine("Mensaje enviado: " & message)
                End If
                Return message
            Catch ex As Exception
                Return AddToLog("SendMesssage@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Function

        Public Class TCPServer
            Private server As TcpListener
            Private client As TcpClient
            Private clientStream As NetworkStream
            Private thread As Thread
            Private isListening As Boolean

            Public Event MessageReceived As EventHandler(Of String)

            Public Sub New()
                server = New TcpListener(IPAddress.Any, 13120)
                isListening = False
            End Sub

            ' Método para iniciar el servidor
            Public Sub StartServer()
                server.Start()
                isListening = True
                thread = New Thread(AddressOf ListenForClients)
                thread.Start()
            End Sub

            ' Método para detener el servidor
            Public Sub StopServer()
                isListening = False
                server.Stop()
            End Sub

            ' Método para escuchar clientes y aceptar la conexión
            Private Sub ListenForClients()
                While isListening
                    If server.Pending() Then
                        client = server.AcceptTcpClient()
                        clientStream = client.GetStream()
                        Console.WriteLine("Cliente conectado.")
                        ' Iniciar hilo para recibir mensajes
                        Dim readThread As New Thread(AddressOf ReadMessages)
                        readThread.Start()
                    End If
                    Thread.Sleep(100)
                End While
            End Sub

            ' Método para leer los mensajes del cliente
            Private Sub ReadMessages()
                Dim buffer(1024) As Byte
                While isListening
                    Try
                        If clientStream.DataAvailable Then
                            Dim bytesRead As Integer = clientStream.Read(buffer, 0, buffer.Length)
                            If bytesRead > 0 Then
                                Dim message As String = Encoding.UTF8.GetString(buffer, 0, bytesRead)
                                RaiseEvent MessageReceived(Me, message)
                            End If
                        End If
                        Thread.Sleep(100)
                    Catch ex As Exception
                        Console.WriteLine("Error leyendo mensaje: " & ex.Message)
                    End Try
                End While
            End Sub

            ' Método para enviar un mensaje al cliente
            Public Sub SendMessageToClient(message As String)
                If clientStream IsNot Nothing Then
                    Dim data As Byte() = Encoding.UTF8.GetBytes(message)
                    clientStream.Write(data, 0, data.Length)
                    Console.WriteLine("Mensaje enviado: " & message)
                End If
            End Sub
        End Class

    End Module

End Namespace