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
        Dim Nickname As String = "Boro-Comm"
        Dim senderSymbol As String = "> "
        Dim IsConnected As Boolean = False
        Dim SERVIDOR As TcpListener
        Dim CLIENTES As New Hashtable
        Dim THREADSERVIDOR As Thread
        Dim CLIENTEIP As IPEndPoint

        Private Structure NUEVOCLIENTE
            Public SOCKETCLIENTE As Socket
            Public THREADCLIENTE As Thread
            Public prefix As String
            Public MENSAJE As String
        End Structure

        Sub StartServer()
            Try
                SERVIDOR = New TcpListener(IPAddress.Any, 13120)
                SERVIDOR.Start()
                THREADSERVIDOR = New Thread(AddressOf ESCUCHAR)
                THREADSERVIDOR.Start()
                IsConnected = True
            Catch ex As Exception
                AddToLog("StartServer@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub
        Sub StopServer()
            Try
                ENVIARTODOS("boro_comm|DISCONNECTED_BY_OWN")
                CERRARTODO()
                SERVIDOR.Stop()
                THREADSERVIDOR.Abort()
                IsConnected = False
            Catch ex As Exception
                AddToLog("StopServer@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub

        Public Sub ESCUCHAR()
            Dim CLIENTE As New NUEVOCLIENTE
            While True
                Try
                    CLIENTE.SOCKETCLIENTE = SERVIDOR.AcceptSocket
                    CLIENTEIP = CLIENTE.SOCKETCLIENTE.RemoteEndPoint
                    CLIENTE.THREADCLIENTE = New Thread(AddressOf LEER)
                    CLIENTES.Add(CLIENTEIP, CLIENTE)
                    NUEVACONEXION(CLIENTEIP)
                    CLIENTE.THREADCLIENTE.Start()
                Catch ex As Exception
                End Try
            End While
        End Sub
        Public Sub LEER()
            Dim CLIENTE As New NUEVOCLIENTE
            Dim DATOS() As Byte
            Dim IP As IPEndPoint = CLIENTEIP
            CLIENTE = CLIENTES(IP)
            While True
                If CLIENTE.SOCKETCLIENTE.Connected Then
                    DATOS = New Byte(100) {}
                    Try
                        If CLIENTE.SOCKETCLIENTE.Receive(DATOS, DATOS.Length, 0) > 0 Then
                            CLIENTE.MENSAJE = Encoding.UTF7.GetString(DATOS)
                            CLIENTES(IP) = CLIENTE
                            DATOSRECIBIDOS(IP)
                        Else

                            Exit While
                        End If
                    Catch ex As Exception

                        Exit While
                    End Try
                End If
            End While
            Call CERRARTHREAD(IP)
        End Sub
        Public Function SendMesssage(ByVal mensaje As String, Optional ByVal UsePrefix As Boolean = True) As String
            Try
                If IsConnected Then
                    If UsePrefix Then
                        ENVIARTODOS(Nickname & senderSymbol & mensaje)
                    Else
                        ENVIARTODOS(mensaje)
                    End If
                    Return mensaje
                Else
                    Return "Can't send."
                End If
            Catch ex As Exception
                Return AddToLog("SendMesssage@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Function

        Public Sub ENVIARUNO(ByVal IDCliente As IPEndPoint, ByVal Datos As String) ' A UN CLIENTE
            Try
                Dim Cliente As NUEVOCLIENTE
                Cliente = CLIENTES(IDCliente)
                Cliente.SOCKETCLIENTE.Send(Encoding.UTF7.GetBytes(Datos))
            Catch ex As Exception
                AddToLog("ENVIARUNO@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub
        Public Sub ENVIARTODOS(ByVal Datos As String, Optional ByVal usePrefix As Boolean = True) 'A TODOS LOS CLIENTES
            Try
                If usePrefix Then
                    Datos = Datos.Replace(Nickname & senderSymbol, Nothing)
                End If
                Dim CLIENTE As NUEVOCLIENTE
                For Each CLIENTE In CLIENTES.Values
                    CLIENTE.SOCKETCLIENTE.Send(Encoding.UTF7.GetBytes(Datos))
                Next
            Catch ex As Exception
                AddToLog("ENVIARTODOS@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub

        Private Sub NUEVACONEXION(ByVal IDTerminal As IPEndPoint)
            'Cuando nueva conexion:
            '   ENVIA: "CONN|IP:PORT"
            Try
                ENVIARTODOS("COMM|" & IDTerminal.Address.ToString & ":" & IDTerminal.Port)
            Catch ex As Exception
            End Try
        End Sub
        Private Sub CONEXIONTERMINADA(ByVal IDTerminal As IPEndPoint)
            'Cuando desconexion:
            '   ENVIA: "DESC|IP:PORT"
            Try
                ENVIARTODOS("DESC|" & IDTerminal.Address.ToString & ":" & IDTerminal.Port)
            Catch ex As Exception
            End Try
        End Sub

        Public Sub CERRARUNO(ByVal IDCliente As IPEndPoint)
            Try
                Dim CLIENTE As NUEVOCLIENTE
                CLIENTE = CLIENTES(IDCliente)
                CLIENTE.SOCKETCLIENTE.Close()
                CLIENTE.THREADCLIENTE.Abort()
            Catch ex As Exception
                AddToLog("CERRARUNO@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub
        Public Sub CERRARTODO()
            Try
                Dim CLIENTE As NUEVOCLIENTE
                For Each CLIENTE In CLIENTES.Values
                    CLIENTE.SOCKETCLIENTE.Close()
                    CLIENTE.THREADCLIENTE.Abort()
                Next
                IsConnected = False
            Catch ex As Exception
                AddToLog("CERRARTODO@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub

        Private Sub DATOSRECIBIDOS(ByVal IDTerminal As IPEndPoint)
            Try
                'cuando recibe datos
                '   ENVIA EL DATO A TODOS
                '   PROCESA EL DATO
                '       ENVIA LA RESPUESTA A TODOS
                Dim mensaje As String = OBTENERDATOS(IDTerminal).Replace(vbNullChar, Nothing)
                Dim remitente As String = IDTerminal.Address.ToString & ":" & IDTerminal.Port

                ENVIARTODOS(remitente & senderSymbol & mensaje, False)

                If mensaje <> Nothing And mensaje <> "" And mensaje.Length > 1 Then
                    ENVIARTODOS(Nickname & senderSymbol & Network.CommandManager.CommandManager(mensaje))
                End If
            Catch ex As Exception
                AddToLog("DATOSRECIBIDOS@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub
        Public Function OBTENERDATOS(ByVal IDCliente As IPEndPoint) As String
            Try
                Dim CLIENTE As NUEVOCLIENTE
                CLIENTE = CLIENTES(IDCliente)
                Return CLIENTE.MENSAJE
            Catch ex As Exception
                Return AddToLog("OBTENERDATOS@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Function
        Public Function PONERDATOS(ByVal IDCliente As IPEndPoint) As String
            Try
                Dim CLIENTE As NUEVOCLIENTE
                CLIENTE = CLIENTES(IDCliente)
                Return CLIENTE.MENSAJE
            Catch ex As Exception
                Return AddToLog("PONERDATOS@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Function

        Public Sub CERRARTHREAD(ByVal IP As IPEndPoint)
            Try
                Dim CLIENTE As NUEVOCLIENTE = CLIENTES(IP)
                CLIENTE.THREADCLIENTE.Abort()
            Catch ex As Exception
                CLIENTES.Remove(IP)
                AddToLog("CERRARTHREAD@Boro_Comm::Connector", "Error: " & ex.Message, True)
            End Try
        End Sub
    End Module

End Namespace
'TODO:
'   se debe crear una forma de poner prefix a los nuevos usuarios.
'       una forma de clase que contenga a los clientes, y asi poder acceder a sus metodos
'       ahora esta usandose Hashtable, no se si se pueda hacer la conversion
