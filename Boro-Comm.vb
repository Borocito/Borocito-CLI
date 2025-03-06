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

        Private listener As TcpListener
        Private client As TcpClient
        Private stream As NetworkStream
        Private reader As System.IO.StreamReader
        Private writer As System.IO.StreamWriter
        Private listenClientsThread As Thread
        Private listenMessagesThread As Thread

        ' Evento de botón para iniciar el servidor
        Sub StartServer()
            Try
                ' Configura el servidor TCP para escuchar en el puerto indicado
                listener = New TcpListener(IPAddress.Any, 13120)
                listener.Start()
                Console.WriteLine("Servidor iniciado...")

                ' Crea un hilo que se encargue de aceptar conexiones
                listenClientsThread = New Thread(AddressOf ListenForClients)
                listenClientsThread.IsBackground = True
                listenClientsThread.Start()
            Catch ex As Exception
                AddToLog("StartServer@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub
        Sub StopServer()
            Try
                ' Cierra los recursos del servidor y del cliente
                If client IsNot Nothing Then client.Close()
                If listener IsNot Nothing Then listener.Stop()
            Catch ex As Exception
                AddToLog("StopServer@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub

        ' Método que espera y acepta una conexión entrante
        Private Sub ListenForClients()
            While True
                Try
                    ' Espera una conexión del cliente
                    client = listener.AcceptTcpClient()
                    Console.WriteLine("Cliente conectado.")

                    ' Crea un stream para la comunicación
                    stream = client.GetStream()
                    reader = New System.IO.StreamReader(stream)
                    writer = New System.IO.StreamWriter(stream)
                    writer.AutoFlush = True

                    ' Comienza a escuchar los mensajes del cliente
                    listenMessagesThread = New Thread(AddressOf ListenForMessages)
                    listenMessagesThread.IsBackground = True
                    listenMessagesThread.Start()
                    'ListenForMessages()
                Catch ex As Exception
                    AddToLog("ListenForClients@Boro_Comm::Connector", "Error: " & ex.Message, True)
                End Try
            End While
        End Sub
        ' Método para escuchar los mensajes enviados por el cliente
        Private Sub ListenForMessages()
            While True
                Try
                    ' Lee el mensaje del cliente
                    Dim message As String = reader.ReadToEnd()
                    If message IsNot Nothing Then
                        Console.WriteLine("Mensaje recibido: " & message)
                    End If
                Catch ex As Exception
                    AddToLog("ListenForMessages@Boro_Comm::Connector", "Error: " & ex.Message, True)
                    Exit While
                End Try
            End While
        End Sub

        ' Método para enviar un mensaje al cliente
        Function SendMesssage(message As String) As String
            Try
                If client IsNot Nothing AndAlso writer IsNot Nothing Then
                    writer.WriteLine(message)
                    Console.WriteLine("Mensaje enviado: " & message)
                End If
                Return message
            Catch ex As Exception
                Return AddToLog("SendMesssage@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Function

    End Module

End Namespace