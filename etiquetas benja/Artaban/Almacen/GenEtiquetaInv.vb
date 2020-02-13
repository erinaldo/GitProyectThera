﻿Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports System.IO

Public Class GenEtiquetaInv
    Public Tipo = ""
    Public Dev As Integer = 0
    Dim Sql = ""
    Dim SKU As String = ""
    Dim idEtiqueta As String = ""
    Dim Cantidad As String = ""
    Dim Pedido As String = ""
    Dim Cliente As String = ""
    Dim Tela As String = ""
    Dim OP As String = ""
    Dim oConexInfo As New CrystalDecisions.Shared.TableLogOnInfo
    Private Sub GenEtiquetaInv_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        If Dev = 0 Then
            Me.Text = "Generador de Etiquetas para Producto Terminado"
        Else
            Me.Text = "Generador de Etiquetas por Devolucion"
        End If
        obtener_impresoras(cbImpresora)
        Sql = "Select SKU, Descripcion from catArticulosPT where tipo in(1,2,3,4,5,10,15,16) order by Descripcion" '& Tipo & " order by Descripcion"
        Dim dsPT As New DataTable
        Dim daPT As New SqlDataAdapter
        Dim cmdPT As New SqlCommand(Sql, Conexion01)
        If Conexion01.State = ConnectionState.Closed Then Conexion01.Open()
        daPT.SelectCommand = cmdPT
        daPT.Fill(dsPT)
        cbCodigo.Properties.DataSource = dsPT
        cbCodigo.Properties.DisplayMember = "SKU"
        cbCodigo.Properties.ValueMember = "SKU"
        'cbCodigo.Properties.Columns(0).Visible = False
        cbDesc.Properties.DataSource = dsPT
        cbDesc.Properties.DisplayMember = "Descripcion"
        cbDesc.Properties.ValueMember = "SKU"
        bGenerar.Enabled = True
        oConexInfo.ConnectionInfo.ServerName = DatosCon.Server
        oConexInfo.ConnectionInfo.DatabaseName = DatosCon.DataBase
        oConexInfo.ConnectionInfo.UserID = DatosCon.dbUser
        oConexInfo.ConnectionInfo.Password = DatosCon.dbPW
        'cbDesc.Properties.Columns(1).Visible = False
    End Sub

    Private Sub GenEtiquetaInv_KeyPress(sender As System.Object, e As System.Windows.Forms.KeyPressEventArgs) Handles MyBase.KeyPress
        Tab(e)
    End Sub

    Private Sub bSalir_Click(sender As System.Object, e As System.EventArgs) Handles bSalir.Click
        Me.Close()
    End Sub

    Private Sub GenEtiquetaInv_FormClosing(sender As System.Object, e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        FrmMain.Visible = True
    End Sub

    Private Sub cbCodigo_EditValueChanging(sender As System.Object, e As DevExpress.XtraEditors.Controls.ChangingEventArgs) Handles cbCodigo.EditValueChanging

    End Sub

    Private Sub cbDesc_EditValueChanging(sender As System.Object, e As DevExpress.XtraEditors.Controls.ChangingEventArgs) Handles cbDesc.EditValueChanging

    End Sub

    Private Sub cbCodigo_EditValueChanged(sender As System.Object, e As System.EventArgs) Handles cbCodigo.EditValueChanged
        If Not cbCodigo.EditValue = cbDesc.EditValue Then ' si los registros de la descripcion es igual al del codigo no realiza cambios
            cbDesc.EditValue = cbCodigo.EditValue
        End If
    End Sub

    Private Sub cbDesc_EditValueChanged(sender As System.Object, e As System.EventArgs) Handles cbDesc.EditValueChanged
        If Not cbDesc.EditValue = cbCodigo.EditValue Then ' si los registros del codigo es igual al de la descripcion no realiza cambios
            cbCodigo.EditValue = cbDesc.EditValue
            '    e.Cancel = False
        End If
    End Sub

    Private Sub bGenerar_Click(sender As System.Object, e As System.EventArgs) Handles bGenerar.Click
        If cbCodigo.Text = "" Then
            MsgBox(Msj00051, MsgBoxStyle.Critical, Empresa & " MSJ00051")
            cbCodigo.Focus()
            Exit Sub
        End If
        If eCantidad.Text = "" Then
            MsgBox(Msj00052, MsgBoxStyle.Critical, Empresa & " MSJ00052")
            eCantidad.Focus()
            Exit Sub
        End If
        Progreso.Visible = True
        Progreso.Value = 0
        For x = 1 To Convert.ToInt64(eCantidad.Text)
            If Progreso.Value < CInt(x * 100 / CInt(eCantidad.Value)) Then
                Progreso.Value = Progreso.Value = CInt(x * 100 / CInt(eCantidad.Value))
            End If
            Generar()
        Next
        Progreso.Visible = False
    End Sub

    Private Sub bExcel_Click(sender As System.Object, e As System.EventArgs) Handles bExcel.Click
        Dim xls_cn As New OleDbConnection
        Dim xls_cmd As New OleDbCommand
        Dim xls_reader As New OleDbDataAdapter
        Dim xlsx As String
        Dim patharchivo As New OpenFileDialog
        Dim SQLArmado = ""
        patharchivo.ShowDialog()
        xlsx = patharchivo.FileName

        SQLArmado = "Select top 0 '' Codigo, '' Cantidad, '' Observaciones"
        Try
            If xlsx = "" Then
                MsgBox(Msj00010, MsgBoxStyle.Critical, Empresa & " Msj00010")
                Exit Sub
            Else
                Dim strExtension As String = ""
                Dim nombreXls As String
                nombreXls = Path.GetFileName(xlsx)
                strExtension = Path.GetExtension(xlsx)
                If strExtension = ".xlsx" Then
                    If (File.Exists(xlsx)) Then
                        bExcel.Enabled = False
                        bGenerar.Enabled = False
                        'xls_cn = New OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + xlsx + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=0'")
                        xls_cn = New OleDb.OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " & xlsx & "; Extended Properties='Excel 12.0 Xml;HDR=YES';")
                        Dim dt As New DataTable("Datos")
                        Using xls_cn
                            xls_cn.Open()
                            xls_cmd.CommandText = "SELECT * FROM [Hoja1$]"
                            xls_cmd.Connection = xls_cn
                            xls_reader.SelectCommand = xls_cmd
                            Dim da As New OleDbDataAdapter(xls_cmd)
                            da.Fill(dt)
                            For i = 0 To dt.Rows.Count - 1
                                If Not IsDBNull(dt.Rows(i).Item("Pedido")) Then Pedido = dt.Rows(i).Item("Pedido").ToString Else Pedido = ""
                                If Not IsDBNull(dt.Rows(i).Item("SKU")) Then SKU = dt.Rows(i).Item("SKU").ToString Else SKU = ""
                                If Not IsDBNull(dt.Rows(i).Item("Cliente")) Then Cliente = dt.Rows(i).Item("Cliente").ToString Else Cliente = ""
                                If Not IsDBNull(dt.Rows(i).Item("Cantidad")) Then Cantidad = dt.Rows(i).Item("Cantidad").ToString Else Cantidad = ""
                                If Not IsDBNull(dt.Rows(i).Item("Tela")) Then Tela = dt.Rows(i).Item("Tela").ToString Else Tela = ""
                                If Dev = 1 Then Pedido = "999"
                                If SKU = "" Then
                                    SQLArmado = "Union all Select '' Codigo, '' Cantidad, 'El Registro no contiene Codigo"
                                Else
                                    If Not CodigoValido() Then
                                        SQLArmado = "Union all Select '" + SKU + "' Codigo, '' Cantidad, 'El Código no es valido o está inactivo, favor de verificarlo"
                                    Else
                                        If Cantidad = "" Then
                                            SQLArmado = "Union all Select '" + SKU + "' Codigo, '' Cantidad, 'Cantidad Invalida"
                                        Else
                                            If Not IsNumeric(dt.Rows(i).Item("Cantidad")) Then
                                                SQLArmado = "Union all Select '" + SKU + "' Codigo, '" & Cantidad & "' Cantidad, 'Cantidad Invalida"
                                            Else
                                                MsgBox("OK")
                                            End If
                                        End If
                                    End If
                                End If
                            Next
                        End Using
                    End If
                    'Limpiar()
                Else
                    MsgBox(Msj00011, MsgBoxStyle.Critical, Empresa & " Msj00011")
                End If
            End If
        Catch ex As Exception
            MsgBox("Error" & Chr(13) & Chr(13) & ex.Message)
        End Try
        Dim dtGrid As New System.Data.DataTable
        Dim daGrid As New SqlDataAdapter
        Dim cmdGrid As New SqlCommand(SQLArmado, Conexion01)
        If Conexion01.State = ConnectionState.Closed Then Conexion01.Open()
        daGrid.SelectCommand = cmdGrid
        daGrid.Fill(dtGrid)
        gCatalogo.DataSource = dtGrid
        Timer1.Enabled = True
    End Sub
    Private Sub Generar()
        Try
            OP = CStr(Microsoft.VisualBasic.DateAndTime.Year(Now)).Substring(2, 2) & Format(Microsoft.VisualBasic.DateAndTime.Month(Now), "00") & Format(Microsoft.VisualBasic.DateAndTime.Day(Now), "00") & Tipo
            If Dev = 1 Then Pedido = "999"
            Dim cmd As New SqlCommand("SpGenEtiquetas", Conexion01)
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.Add("@OPC", SqlDbType.Int).Value = (3 + Dev)
            cmd.Parameters.Add("@OP", SqlDbType.BigInt).Value = CInt(OP)
            cmd.Parameters.Add("@SKU", SqlDbType.VarChar, 15).Value = cbCodigo.Text
            cmd.Parameters.Add("@Cliente", SqlDbType.VarChar, 250).Value = Cliente
            cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = CInt(eCantidad.Text)
            cmd.Parameters.Add("@Pedido", SqlDbType.VarChar, 50).Value = Pedido
            cmd.Parameters.Add("@Tela", SqlDbType.VarChar, 250).Value = Tela
            cmd.Parameters.Add("@JobNum", SqlDbType.VarChar, 6).Value = ""
            cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = Usuario
            cmd.Parameters.Add("@Resultado", SqlDbType.VarChar, 1)
            cmd.Parameters.Add("@Msj", SqlDbType.VarChar, 8000)
            cmd.Parameters("@Resultado").Direction = ParameterDirection.Output
            cmd.Parameters("@Msj").Direction = ParameterDirection.Output
            If Conexion01.State = ConnectionState.Closed Then Conexion01.Open()
            cmd.ExecuteNonQuery()
            If cmd.Parameters("@Resultado").Value.ToString() = "1" Then
                MsgBox(cmd.Parameters("@Msj").Value.ToString() & Chr(13) & Msj00020, MsgBoxStyle.Critical, Empresa & " MSJ00020")
                Exit Sub
            End If
            Imprimir(Convert.ToInt64(cmd.Parameters("@Msj").Value))
        Catch e As Exception
            MsgBox(Msj00020 & Chr(13) + e.Message, MsgBoxStyle.Critical, Empresa & " MSJ00020")
            Exit Sub
        End Try
        idEtiqueta = "0"
        'Imprimir(1) 'Impresion por primera vez
        'MsgBox(Msj00021, MsgBoxStyle.Information, Empresa & " MSJ00021") 'Se guardo de manera correcta
    End Sub

    Private Sub Imprimir(ByVal ID As Int64)
        'Dim contador As Int16
        'contador = 0
        'Dim rpt As New EtPT
        'Try
        '    For x = 0 To rpt.Database.Tables.Count - 1
        '        rpt.Database.Tables(x).ApplyLogOnInfo(oConexInfo)
        '        rpt.Database.Tables(x).ApplyLogOnInfo(oConexInfo)
        '    Next
        '    rpt.SetParameterValue("@Consecutivo", ID)
        '    rpt.PrintOptions.PrinterName = cbImpresora.Text
        '    rpt.PrintToPrinter(1, True, 0, 0)
        'Catch ex As Exception
        '    MsgBox(Msj00029 + Chr(13) + ex.Message, MsgBoxStyle.Critical, Empresa & " MSJ00029")
        '    rpt.Dispose() 
        '    Exit Sub
        'End Try
        'rpt.Dispose()
        'Dim CrReport As New CrystalDecisions.CrystalReports.Engine.ReportDocument
        'Try
        '    Dim dtRep As New DataTable
        '    Dim daRep As New SqlDataAdapter
        Dim Sql = "Select EtiquetasProd.SKU, Descripcion, SubString(TipoDesc,1,3)TipoDesc, CodMedida, SubString(MedidaDesc,1,4)MedidaDesc, Consecutivo, Pedido, EtiquetasProd.Tela, " & _
            " 'Piezas: ' +cast(netiqueta as varchar(10)) + ' de ' + cast(TotEtiquetas as varchar(10)) NoPiezas, " & _
            " CompNom, UniResorte, case when len(SKUCTE)> 0 then concat('*',SKUCte, '*') else '' end SKUCte " & _
            " from etiquetasProd with(noLock) left outer join catArticulosPT with(noLock) on etiquetasprod.sku = catArticulosPT.sku left outer join catTipos with(NoLock) " & _
            " on CatArticulosPT.tipo = catTipos.idtipo left outer join catMedidas with(NoLock) on catARticulosPT.medida = CatMEdidas.Codmedida " & _
            " where Consecutivo = " & ID.ToString 
        If Dev = 1 Then
            Dim Reporte As New EtPTD
            ImprimeCR(cbImpresora.Text, Sql, Reporte, 1, Nothing, False)
            Reporte.Dispose()
        Else
            Dim reporte As New EtPT
            ImprimeCR(cbImpresora.Text, Sql, reporte, 1, Nothing, False)
            reporte.Dispose()
        End If
        'Dim CMD As New SqlCommand(Sql, Conexion01)
        'If Conexion01.State = False Then Conexion01.Open()
        'daRep.SelectCommand = CMD
        'daRep.Fill(dtRep)
        'CrReport = New EtPT
        'CrReport.SetDataSource(dtRep)
        'CrReport.PrintOptions.PrinterName = cbImpresora.Text
        'CrReport.PrintToPrinter(1, True, 0, 0)
        'Catch ex As Exception
        '    MsgBox(Msj00029 + Chr(13) + ex.Message, MsgBoxStyle.Critical, Empresa & " MSJ00029")
        '    CrReport.Dispose()
        'End Try
        'CrReport.Dispose()
    End Sub

    Private Function CodigoValido() As Boolean
        CodigoValido = False
        Dim dtCod As New System.Data.DataTable
        Dim daCod As New SqlDataAdapter
        Dim cmdCod As New SqlCommand("Select * from CatArticulosPT with(Nolock) where Estatus = 1 and tipo in(1,2,3)  and SKU = '" & SKU & "'", Conexion01)
        If Conexion01.State = ConnectionState.Closed Then Conexion01.Open()
        daCod.SelectCommand = cmdCod
        daCod.Fill(dtCod)
        If dtCod.Rows.Count > 0 Then CodigoValido = True
    End Function

    Private Sub Timer1_Tick(sender As System.Object, e As System.EventArgs) Handles Timer1.Tick
        bGenerar.Enabled = True
        bExcel.Enabled = True
        bSalir.Enabled = True
        Timer1.Enabled = False
    End Sub
     
End Class