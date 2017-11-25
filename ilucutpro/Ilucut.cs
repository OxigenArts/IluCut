using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using Cairo;
namespace ilucutpro
{
	public class Ilucut
	{
		public String fileCont,filePath,toCut;
		public bool isOpen,preview;
		public MainWindow ventana;
		public int width, height;
		public int count,cant, columns,rows;
		public double zoom,scaleX,scaleY,rel,spaceX, spaceY;
		public int despX, despY;
		public int compX,compY;
		public int sheetW, sheetH,sheetMargin;
		public string serialPort;
		public List<List<PointD>> formas = new List<List<PointD>>();

		public Ilucut (MainWindow v)
		{
			this.ventana = v;
			this.isOpen = false;
			this.zoom = 1;
			this.sheetW = 210;
			this.sheetH = 297;
			this.scaleX = 1;
			this.scaleY = 1;
			this.sheetMargin = 1;
			this.cant = 1;
			this.columns = 1;
			this.rows = 1;
			this.spaceX = 40;
			this.spaceY = 40;
			this.preview = true;
		}
		public void msgShow(String msg){
			MessageDialog md = new MessageDialog (this.ventana, 
				DialogFlags.DestroyWithParent, MessageType.Info, 
				ButtonsType.Close, msg);
			md.Run ();
			md.Destroy ();
		}
		private void cargarHpgl(){
			formas = new List<List<PointD>>();
			try {
				String[] ordenes = fileCont.Split (';');
				int x,y;
				int i = -1;

				foreach (var orden in ordenes) {
					if(orden != "PU0,0" && orden.Length > 2){
						switch (orden.Substring(0,2)) {
						case "PU":
							formas.Add (new List<PointD>());
							i++;
							string[] od = orden.Substring(2).Split (',');
							x = Convert.ToInt32(od [0]);
							y = Convert.ToInt32(od [1]);
							formas [i].Add (new PointD(x, y));
							break;
						case "PD":
							string[] oda = orden.Substring(2).Split (',');
							for (int k = 0; k < oda.Length/2; k++) {
								x = Convert.ToInt32(oda[k*2]);
								y = Convert.ToInt32(oda[k*2+1]);
								formas [i].Add (new PointD(x, y));
							}
							break;
						default:
							
						break;
						}
					}
				}

				this.width = 0;
				this.height = 0;
				foreach (var forma in formas) {
					foreach (var punto in forma) {
						if(punto.X > this.width)
							this.width = (int)punto.X;
						if(punto.Y > this.height)
							this.height = (int)punto.Y;
					}
				}
				rel = this.height/this.width;

				this.rotar();


				this.isOpen = true;
			} catch (Exception ex) {
				msgShow ("Archivo dañado.\nError: "+ex.Message);
			}

		}
		public void espejar(){
			List<List<PointD>> nformas = new List<List<PointD>>();
			int forma_i = 0;
			foreach (var forma in formas) {
				nformas.Add (new List<PointD>());
				foreach (var punto in forma) {
					nformas [forma_i].Add (new PointD (punto.X, height-punto.Y));
				}
				forma_i++;
			}
			formas = nformas;
		}
		public void rotar(){
			List<List<PointD>> nformas = new List<List<PointD>>();
			int forma_i = 0;
			foreach (var forma in formas) {
				nformas.Add (new List<PointD>());
				foreach (var punto in forma) {
					nformas [forma_i].Add (new PointD (punto.Y, width-punto.X));
				}
				forma_i++;
			}
			formas = nformas;
			int aux;
			aux = this.width;
			this.width = this.height;
			this.height = aux;
			rel = this.height/this.width;
		}
		public int maxColumn(){
			return (int)((this.sheetW * 40) / (this.width*scaleX + this.spaceX));
		}
		public int maxRow(){
			return (int)((this.sheetH * 40) / (this.height*scaleY + this.spaceY));
		}
		public int maxCant(){
			return (int)(this.maxRow() * this.maxColumn ());
		}
		private void cargarIlu(){
			formas = new List<List<PointD>>();
			try {
				String[] ordenes = fileCont.Split (';');
				int i = -1;
				this.width = 0;
				this.height = 0;
				foreach (var orden in ordenes) {
					if (orden == "p") {
						formas.Add (new List<PointD>());
						i++;
					} else {
						string[] od = orden.Split (',');
						int x = Convert.ToInt32(od [0]);
						int y = Convert.ToInt32(od [1]);
						formas [i].Add (new PointD(x, y));
						if(x > this.width)
							this.width = x;
						if(y > this.height)
							this.height = y;
						
					}
				}
				rel = this.height/this.width;
				this.isOpen = true;
			} catch (Exception ex) {
				msgShow ("Archivo dañado.\nError: "+ex.Message);
			}

		}
		public void openFile(){
			//Abre cuadro de dialogo;
			Gtk.FileChooserDialog filechooser =
				new Gtk.FileChooserDialog("Choose the file to open",
					this.ventana,
					FileChooserAction.Open,
					"Cancel",ResponseType.Cancel,
					"Open",ResponseType.Accept);
			//si abrio un archivo
			if (filechooser.Run() == (int)ResponseType.Accept) 
			{
				this.filePath = filechooser.Filename;
				String fileExt = System.IO.Path.GetExtension (this.filePath);
				//destruir cuadro de dialogo

				switch (fileExt) {
				case ".hpgl"://si es hpgl
					this.fileCont = File.ReadAllText (filechooser.Filename);
					cargarHpgl ();
				
					break;
				case ".ilu"://si es ilu
					this.fileCont = File.ReadAllText (filechooser.Filename);
					cargarIlu ();

					break;
				default:
					msgShow ("El archivo no tiene el formato requerido (hpgl o ilu)");
					break;
				}


			}
			filechooser.Destroy();
		}
		public void preparar(){


			if (this.isOpen) {
				toCut = "IN;SP1;";
				count = 0;//formas dibujadas
				this.rows = (int)(this.cant / this.columns)+1;
				for (int i = 0; i < rows; i++) {//filas
					for (int j = 0; j < columns; j++) {//columnas
						if (count == cant)
							break;
						//dibuja el archivo 
						foreach (var forma in formas) {
							PointD p0 = new PointD (sheetMargin*zoom+spaceX*j*zoom+(forma[0].Y+(height)*j)*zoom*scaleY,sheetMargin*zoom+spaceY*i*zoom+(forma[0].X+(width)*i)*zoom*scaleX);

							//PointD p0 = new PointD (area_w-forma[0].X*zoom*scaleX/40-sheetMargin*zoom-(width*scaleX*zoom-spaceX*zoom)*i,area_h-forma[0].Y*zoom*scaleY/40-sheetMargin*zoom-(height*scaleY*zoom-spaceY*zoom)*j);
							//g.MoveTo (p0);
							toCut += "PU" + ((int)p0.X).ToString () + "," + ((int)p0.Y).ToString ()+";PD";
							foreach (var punto in forma) {
								PointD pn = new PointD (sheetMargin*zoom+spaceX*j*zoom+(punto.Y+(height)*j)*zoom*scaleY,sheetMargin*zoom+spaceY*i*zoom+(punto.X+(width)*i)*zoom*scaleX);
								//g.LineTo(pn);
								toCut += ((int)pn.X).ToString () + "," + ((int)pn.Y).ToString () + ",";

							}
							toCut += ";";
						}
						//suma 1
						count++;
					}
				}
				toCut.Replace (",;", ";");
				//toCut += ";PU;";

			}
		}
		private void dibujarCuadrado(Cairo.Context g,int x,int y,int width,int height,Color fondo,Color linea,int grosor = 1){
			g.LineWidth = grosor;
			PointD p1,p2,p3,p4;

			p1 = new PointD (x,y);
			p2 = new PointD (width,y);
			p3 = new PointD (width,height);
			p4 = new PointD (x,height);
			g.MoveTo (p1);
			g.LineTo (p2);
			g.LineTo (p3);
			g.LineTo (p4);
			g.LineTo (p1);
			g.ClosePath ();
			g.Color = fondo;
			g.FillPreserve ();
			g.Color = linea;
			g.Stroke ();
		}

		public void dibujarVista(DrawingArea area){
			using (Cairo.Context g = Gdk.CairoHelper.Create (area.GdkWindow)) {
				//draw white square
				g.Antialias = Antialias.None;
				int area_w = area.Allocation.Width;
				int area_h = area.Allocation.Height;
				dibujarCuadrado (g, 0, 0, area_w, area_h, new Color (0.3, 0.3, 0.3), new Color (0, 0, 0), 2);
				if (preview) {
					//draw A4 Sheet
					g.Color = new Color(0, 1, 1);
					g.SetFontSize(15*zoom);
					String txt_sheet = "Hoja " + sheetW + "x" + sheetH + "mm.";
					TextExtents te = g.TextExtents(txt_sheet);
					g.MoveTo(area_w - sheetW /2*zoom -te.Width  / 2 - te.XBearing,
						area_h - sheetH*zoom -10*zoom - te.Height / 2 - te.YBearing);
					g.ShowText(txt_sheet);

					int a4_w=(int)(this.sheetW*this.zoom);
					int a4_h=(int)(this.sheetH*this.zoom);
					dibujarCuadrado (g, area_w, area_h, area_w-a4_w, area_h-a4_h, new Color (1,1,1), new Color (0, 0, 0), 1);
					g.Antialias = Antialias.Default;
					//Dibujar formas
					g.Color = new Color(1,0,0);
					if (this.isOpen) {
						count = 0;//formas dibujadas
						this.rows = (int)(this.cant / this.columns)+1;
						for (int i = 0; i < rows; i++) {//filas
							for (int j = 0; j < columns; j++) {//columnas
								if (count == cant)
									break;
								//dibuja el archivo 
								foreach (var forma in formas) {
									PointD p0 = new PointD (area_w-sheetMargin*zoom-spaceX/40*j*zoom-(forma[0].Y+(height)*j)*zoom*scaleY/40,area_h-sheetMargin*zoom-spaceY/40*i*zoom-(forma[0].X+(width)*i)*zoom*scaleX/40);

									//PointD p0 = new PointD (area_w-forma[0].X*zoom*scaleX/40-sheetMargin*zoom-(width*scaleX*zoom-spaceX*zoom)*i,area_h-forma[0].Y*zoom*scaleY/40-sheetMargin*zoom-(height*scaleY*zoom-spaceY*zoom)*j);
									g.MoveTo (p0);
									foreach (var punto in forma) {
										PointD pn = new PointD (area_w-sheetMargin*zoom-spaceX/40*j*zoom-(punto.Y+(height)*j)*zoom*scaleY/40,area_h-sheetMargin*zoom-spaceY/40*i*zoom-(punto.X+(width)*i)*zoom*scaleX/40);
										g.LineTo(pn);
									}
									g.Color = new Color(0,0,0,0.1);
									//g.ClosePath ();
									g.FillPreserve ();
									g.Color = new Color(0,0,1);
									g.Stroke ();
								}
								//suma 1
								count++;
							}
						}

					}
					g.Antialias = Antialias.None;

				}
				//draw black border
				dibujarCuadrado (g, 0, 0, area_w, area_h, new Color (0, 0, 0,0), new Color (0, 0, 0), 2);

				g.GetTarget ().Dispose ();
				g.Dispose ();
			}
		}
	}
}

