using System;
using Gtk;
using ilucutpro;
public partial class MainWindow: Gtk.Window
{
	Ilucut ilucut;
	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
		drawingarea1.DoubleBuffered = true;
		ilucut = new Ilucut (this);
		//auto zoom
		autoZoom();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void vistaPrevia (object o = null, ExposeEventArgs args= null)
	{
		ilucut.dibujarVista (drawingarea1);
	}

	protected void OnFixed2ResizeChecked (object sender, EventArgs e)
	{
		drawingarea1.SetSizeRequest (this.Allocation.Size.Width - 200,drawingarea1.Allocation.Size.Height);
	}

	protected void OnHscale1ValueChanged (object sender, EventArgs e)
	{
		ilucut.zoom = hscale1.Value/100;
		vistaPrevia ();

	}

	protected void OnSpWidthValueChanged (object sender, EventArgs e)
	{
		ilucut.scaleX = spWidth.Value * 40 /ilucut.width ;
		spHeight.Value = ilucut.rel * spWidth.Value;
		vistaPrevia ();
	}

	protected void OnSpHeightValueChanged (object sender, EventArgs e)
	{
		ilucut.scaleY = spHeight.Value * 40 /ilucut.height ;
		vistaPrevia ();
	}

	protected void OnSpCantValueChanged (object sender, EventArgs e)
	{
		ilucut.cant = (int)spCant.Value;
		vistaPrevia ();

	}

	protected void OnSpColumnsValueChanged (object sender, EventArgs e)
	{
		ilucut.columns = (int)spColumns.Value;
		vistaPrevia ();
	}

	protected void OnSpSpaceXValueChanged (object sender, EventArgs e)
	{
		ilucut.spaceX = spSpaceX.Value * 40;
		spSpaceY.Value = spSpaceX.Value;
		vistaPrevia ();


	}

	protected void OnSpSpaceYValueChanged (object sender, EventArgs e)
	{
		ilucut.spaceY = spSpaceY.Value*40;
		vistaPrevia ();
	}
		
	protected void autoZoom (object sender = null, EventArgs e =null)
	{
		hscale1.Value = drawingarea1.Allocation.Height * 100 / (ilucut.sheetH+23);
		if(drawingarea1.Allocation.Width<(ilucut.sheetW+5)*ilucut.zoom)
			hscale1.Value = drawingarea1.Allocation.Width * 100 / ilucut.sheetW-5;
	}

	protected void OnButton1Pressed (object sender, EventArgs e)
	{
		int rows = (int)(spCant.Value / spColumns.Value)+1;
		if (rows * spColumns.Value == spCant.Value) {
			spCant.Value += spColumns.Value;
		} else {
			spCant.Value = rows * spColumns.Value;
		}
	}

	protected void OnButton2Pressed (object sender, EventArgs e)
	{
		int rows = (int)(spCant.Value / spColumns.Value);
		if (rows * spColumns.Value == spCant.Value) {
			spCant.Value -= spColumns.Value;
		} else {
			spCant.Value = rows * spColumns.Value;
		}
	}
		
	protected void OnOpenActionActivated (object sender, EventArgs e)
	{
		ilucut.openFile ();
		spCant.Value = 1;
		spColumns.Value = 1;
		spWidth.Value = (double)ilucut.width/40;
		spHeight.Value = (double)ilucut.height/40;
		vistaPrevia ();

	}

	protected void OnSpAnchoValueChanged (object sender, EventArgs e)
	{
		ilucut.sheetW = (int)spAncho.Value;
		autoZoom ();
		vistaPrevia ();
	}

	protected void OnSpAltoValueChanged (object sender, EventArgs e)
	{
		ilucut.sheetH = (int)spAlto.Value;
		autoZoom ();
		vistaPrevia ();
	}

	protected void OnButton4Pressed (object sender, EventArgs e)
	{
		spColumns.Value = ilucut.maxColumn ();
		spCant.Value = ilucut.maxCant ();
		vistaPrevia ();
	}
		
	protected void OnCbPreviewToggled (object sender, EventArgs e)
	{
		if (cbPreview.Active) {
			ilucut.preview = true;
			vistaPrevia ();
		}
		else{
			ilucut.preview = false;
			vistaPrevia ();
		}
	}

	protected void OnButton3Pressed (object sender, EventArgs e)
	{
		autoZoom ();
	}
	protected void OnCdromActionActivated (object sender, EventArgs e)
	{
		ilucut.espejar ();
		vistaPrevia ();
	}
	protected void OnRefreshActionActivated (object sender, EventArgs e)
	{
		ilucut.rotar ();
		vistaPrevia ();
	}


}
