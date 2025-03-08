Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Namespace Boro_Comm

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
                If e.Trim().StartsWith("¡#") Then 'es un mensaje para broadcast only
                    Return SendMesssage(e.Replace("¡#", ""))
                End If
                Return SendMesssage(Network.CommandManager.CommandManager(e))
            Catch ex As Exception
                Return AddToLog("MensajeRecibido@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Function

        ' Método para enviar un mensaje a todos los conectados
        Function SendMesssage(message As String) As String
            Try
                If ServidorTCP IsNot Nothing AndAlso message IsNot Nothing Then
                    ServidorTCP.SendMessageToAllClients(message)
                    Console.WriteLine("Mensaje enviado: " & message)
                End If
                Return message
            Catch ex As Exception
                Return AddToLog("SendMesssage@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Function

        Public Class TCPServer
            Private server As TcpListener
            Private clients As List(Of TcpClient)
            Private clientStreams As List(Of NetworkStream)
            Private isListening As Boolean
            Private thread As Thread

            ' Evento para notificar cuando un cliente envía un mensaje
            Public Event MessageReceived As EventHandler(Of String)

            Public Sub New()
                server = New TcpListener(IPAddress.Any, 13120)
                clients = New List(Of TcpClient)()
                clientStreams = New List(Of NetworkStream)()
                isListening = False
            End Sub

            ' Método para iniciar el servidor
            Public Sub StartServer()
                server.Start()
                isListening = True
                thread = New Thread(AddressOf ListenForClients)
                thread.Start()
                Console.WriteLine("Servidor iniciado...")
            End Sub

            ' Método para detener el servidor
            Public Sub StopServer()
                isListening = False
                server.Stop()
                For Each client As TcpClient In clients
                    client.Close()
                Next
                clients.Clear()
                clientStreams.Clear()
                Console.WriteLine("Servidor detenido.")
            End Sub

            ' Método para escuchar a los clientes y aceptar conexiones
            Private Sub ListenForClients()
                While isListening
                    If server.Pending() Then
                        Dim newClient As TcpClient = server.AcceptTcpClient()
                        clients.Add(newClient)
                        Dim newStream As NetworkStream = newClient.GetStream()
                        clientStreams.Add(newStream)

                        Console.WriteLine("Nuevo cliente conectado.")
                        ' Iniciar un hilo para manejar la comunicación con el nuevo cliente
                        Dim clientThread As New Thread(AddressOf HandleClientCommunication)
                        clientThread.Start(newClient)
                    End If
                    Thread.Sleep(100)
                End While
            End Sub

            ' Método para manejar la comunicación con cada cliente
            Private Sub HandleClientCommunication(client As TcpClient)
                Dim clientStream As NetworkStream = client.GetStream()
                Dim buffer(1024) As Byte
                While isListening
                    Try
                        If clientStream.DataAvailable Then
                            Dim bytesRead As Integer = clientStream.Read(buffer, 0, buffer.Length)
                            If bytesRead > 0 Then
                                Dim message As String = Encoding.UTF8.GetString(buffer, 0, bytesRead)
                                ' Llamar al evento para notificar a otros componentes
                                RaiseEvent MessageReceived(Me, message)
                            End If
                        End If
                        Thread.Sleep(100)
                    Catch ex As Exception
                        Console.WriteLine("Error con el cliente: " & ex.Message)
                        Exit While
                    End Try
                End While
            End Sub

            ' Método para enviar un mensaje a un cliente específico
            Private Sub SendMessageToClient(client As TcpClient, message As String)
                Dim clientStream As NetworkStream = client.GetStream()
                Dim data As Byte() = Encoding.UTF8.GetBytes(message)
                Try
                    clientStream.Write(data, 0, data.Length)
                Catch ex As Exception
                    Console.WriteLine("Error enviando mensaje al cliente: " & ex.Message)
                End Try
            End Sub
            ' Método para enviar un mensaje a todos los clientes conectados
            Public Sub SendMessageToAllClients(message As String)
                Dim data As Byte() = Encoding.UTF8.GetBytes(message)
                For Each clientStream As NetworkStream In clientStreams
                    Try
                        clientStream.Write(data, 0, data.Length)
                    Catch ex As Exception
                        Console.WriteLine("Error enviando mensaje a los clientes: " & ex.Message)
                    End Try
                Next
            End Sub
        End Class

    End Module

End Namespace