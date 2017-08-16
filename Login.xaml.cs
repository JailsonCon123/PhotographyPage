using System;
using System.Collections.Generic;

using Xamarin.Forms;
using SQLite.Net;
using EstruturaTabelas;
using System.Linq;
using System.Threading.Tasks;

namespace TerrasWeb 
{
	public partial class Login : ContentPage
	{
		DadosLogin dados = new DadosLogin ();
		Server server = new Server();
		public static int? idUsuarioLogado;
		public static int? idClientLogado;
		public static int? idClientHolderLogado;
		public static int? idPerfilLogado;
		public static string documento;

		SQLiteConnection db;
		bool internetOk;

		public Login ()
		{
			InitializeComponent ();


			Image imageHeader = new Image{ Aspect = Aspect.AspectFit,HeightRequest = App.ScreenHeight / 3};
			imageHeader.Source = ImageSource.FromResource("Resources.iconeRedeTerras.png");

			logoTerrasWeb.Children.Add (imageHeader);


			db = DBConnector.OpenConnection (); 

			CriaTabelasDaAplicacao (db);
			DBConnector.CloseConnection(db); 
			internetOk = VerificaAcessoInternet ();
			VerificaLoginSalvo ();
//			txbLogin.Text = "comercial@agrotools.com.br";
//			txbSenha.Text = "agro700ls";
			if (!internetOk) {
				btnEsqueciSenha.IsEnabled = false;
			}
			this.btnEsqueciSenha.Clicked += (sender, e) => 
			{
				var page = new EsqueciSenha();
				Navigation.PushModalAsync (page);
			};

			this.btnLogin.Clicked += async (sender, e) => 
			{

				if (this.chkSalvarDados.Checked == true) 
				{

					db = DBConnector.OpenConnection();
					//db.DeleteAll<EstruturaTabelas.Configdev> ();
					EstruturaTabelas.LoginSalvo login = new EstruturaTabelas.LoginSalvo();

					login.login = txbLogin.Text;
					login.senha = txbSenha.Text;
					db.Insert (login);
					DBConnector.CloseConnection(db);


				}
				else 
				{
					db = DBConnector.OpenConnection();
					db.DeleteAll<EstruturaTabelas.LoginSalvo> ();
					DBConnector.CloseConnection(db);
				}
				this.loading.IsVisible = false;
				this.lblStatusText.IsVisible = false;
				if (String.IsNullOrEmpty(this.txbLogin.Text)) 
				{
					this.lblStatusText.Text = AppResources.campoLoginObrigatorio;
					this.lblStatusText.IsVisible = true;
				}
				else if (String.IsNullOrEmpty(this.txbSenha.Text)) 
				{
					this.lblStatusText.Text = AppResources.campoSenhaObrigatorio;
					this.lblStatusText.IsVisible = true;
				}

				else
				{
					
					this.loading.IsVisible = true;

					Logar ();
				}

			};
		}

		void Logar()
		{
			try
			{
				internetOk = VerificaAcessoInternet ();

				if (internetOk) 
				{
					
					Commons.Encryption oEncryption = null;
					oEncryption = new Commons.Encryption ();

					string senhaCriptografada = oEncryption.Encrypt (this.txbSenha.Text).Replace ('/', ' ');

					//Obtem a configuração de url do GlobalAccount. Se nao existir valor usa produçção como default, senao usa o que ta informado na tela de configuração

					db = DBConnector.OpenConnection ();

					EstruturaTabelas.Configdev resultadoConfig = db.Table<EstruturaTabelas.Configdev> ().FirstOrDefault();


					var GetDados = DependencyService.Get<IHelper> ();
					//string urlLoginCompleta = string.Format (AppResources.urlLoginUsuario, this.txbLogin.Text, senhaCriptografada, AppResources.langAuthHeader, AppResources.idSistemaGix, AppResources.app_name);  
					//dados = GetDados.ObtemDadosLogin (urlLoginCompleta);
					dados = GetDados.ObtemDadosLogin (AppResources.urlLoginUsuario, 
														this.txbLogin.Text, senhaCriptografada, 
														AppResources.langAuthHeader, 
														AppResources.idSistemaGix, 
														AppResources.app_name);
							
					if (dados.DataResponse.HasSuccess) 
					{

						int? codigoCliente = dados.ClientCode;
						int? codigoHolder = dados.CompanyHolderId;
						int? idUsuario = dados.UserCode;
						int? idPerfil = dados.UserProfileId;
						documento = dados.Document;
						//int? cpfCliente = oInvoke.Result.
						idUsuarioLogado = idUsuario;
						idClientLogado = codigoCliente;
						idClientHolderLogado = codigoHolder;
						idPerfilLogado = idPerfil;

						string urlLogo = dados.UrlClientLogo;
						//	ObterLogoCliente(urlLogo, codigoHolder.Value);

						var appDependency = DependencyService.Get<IHelper> ();

						//Adiciona http:// na url para enviar serviço
						if (!dados.Host.Contains ("http://")) {
							dados.Host = dados.Host.Insert (0, "http://");
						}

						if (!(dados.Host [dados.Host.Length - 1]).Equals ('/')) {
							dados.Host = dados.Host.Insert (dados.Host.Length, "/");
						}
							
						db = DBConnector.OpenConnection (); 


						#region Obtem e Salva Questionarios
//					string urlServicoQuestionario = "http://www.terramatrix.com.br/services/Services/QuestionnaireService.svc/GetAvailableQuestionnaires/1/1/1";//dados.Host + String.Format (AppResources.sufixoObterQuestionarioService, idUsuario, idPerfil, codigoCliente);
//						List<Questionario> listaQuestionarios = appDependency.ObtemQuestionariosDoServico (urlServicoQuestionario);
//
//						if (listaQuestionarios != null) {
//							db.BeginTransaction ();
//						bool sucessSalvar = Utils.SalvarQuestionariosNoBanco (db, listaQuestionarios, idUsuarioLogado.Value,idClientLogado.Value);
//							if (sucessSalvar) {
//								db.Commit ();
//							} else {
//								db.Rollback ();
//								//LANÇAR AVISO AO USUARIO QUE NAO CONSEGUIU SALVAR QUESTIONARIO
//								//Utils.SendNotification(Resources.GetString(Resource.String.tituloErroBaixarQuestionarios), Resources.GetString(Resource.String.mensagemErroBaixarQuestionarios), 
//								//	this.ApplicationContext);
//							}
//						}
						#endregion

	//					#region Obtem e Salva Roteiro de Visita
	//					string urlServicoPlanoVoo = dados.Host + String.Format (AppResources.sufixoObterPlanoVooService, idUsuario, codigoCliente);
	//					List<PlanoVoo> listaPlanosVoo = appDependency.ObtemPlanosVooDoServico (urlServicoPlanoVoo);
	//					if (listaPlanosVoo != null) {
	//						db.BeginTransaction ();
	//						bool sucessSalvar = Utils.SalvarPlanosVooNoBanco (db, listaPlanosVoo, idUsuario.Value);
	//						if (sucessSalvar) {
	//							db.Commit ();
	//						} else {
	//							db.Rollback ();
	//							//LANÇAR AVISO AO USUARIO QUE NAO CONSEGUIU SALVAR Plano de Voo
	//							//Utils.SendNotification(Resources.GetString(Resource.String.tituloErroBaixarPlanosVoo), Resources.GetString(Resource.String.mensagemErroBaixarPlanosVoo), 
	//							//	this.ApplicationContext);
	//						}
	//					}
	//					#endregion

						EstruturaTabelas.Usuario usuario = new EstruturaTabelas.Usuario ();

						usuario.Login = this.txbLogin.Text;
						usuario.Senha = senhaCriptografada;
						usuario.IdUsuario = idUsuario;
						usuario.IdCliente = codigoCliente;
						usuario.IdHolder = codigoHolder;
						usuario.IdPerfil = idPerfil;
						usuario.CPF = documento;
						usuario.HostCliente = dados.Host;
						usuario.SufixoUrlUpload = AppResources.sufixoUrlUploadData;
						usuario.SufixoUrlAddPoint = AppResources.sufixoUrlAddPoint;

						//Faz select para atualizar registro no BD

						const string selectSql = "select * from usuario where login = ? and idCLiente = ?";
						var resultadoConsulta = db.Query<EstruturaTabelas.Usuario> (selectSql, new String[] {
							this.txbLogin.Text,
							codigoCliente.ToString ()
						});
						if (resultadoConsulta.Count > 0) {
							EstruturaTabelas.Usuario usuarioEncontrado = new EstruturaTabelas.Usuario ();
							usuarioEncontrado = resultadoConsulta.First ();
							usuarioEncontrado.Login = usuario.Login;
							usuarioEncontrado.Senha = usuario.Senha;
							usuarioEncontrado.IdUsuario = usuario.IdUsuario;
							usuarioEncontrado.IdCliente = usuario.IdCliente;
							usuarioEncontrado.IdHolder = usuario.IdHolder;
							usuarioEncontrado.IdPerfil = usuario.IdPerfil;
							usuarioEncontrado.UrlLayer = usuario.UrlLayer;
							usuarioEncontrado.LayerName = usuario.LayerName;
							usuarioEncontrado.CPF = usuario.CPF;



							//Mudar para deixar dinamico
							usuarioEncontrado.HostCliente = usuario.HostCliente;
							usuarioEncontrado.SufixoUrlUpload = usuario.SufixoUrlUpload;
							usuarioEncontrado.SufixoUrlAddPoint = usuario.SufixoUrlAddPoint;

							int sucesso = db.Update (usuarioEncontrado);
							DBConnector.CloseConnection (db);
							var page = new SelectQuestionary();
							Navigation.PushModalAsync(page);
					 
							this.loading.IsVisible = true;

						} else {
							int sucesso = db.Insert (usuario);

							DBConnector.CloseConnection (db);
							var page = new SelectQuestionary();
							Navigation.PushModalAsync(page);
							this.loading.IsVisible = true;

						}
					} 
					else 
					{
						AlertaErroLogin (dados.DataResponse.ErrorMessage);
					}
	//				var root = new SelectQuestionary();
	//				Navigation.PushModalAsync(root);
	//				this.loading.IsVisible = true;
					

				} 
				else 
				{
					db = DBConnector.OpenConnection ();
					//db.DeleteAll<EstruturaTabelas.Usuario> ();
					List<EstruturaTabelas.Usuario> resultadoConsulta = db.Table<EstruturaTabelas.Usuario>().ToList();
					int qtde = resultadoConsulta.Count ();
					//DBConnector.CloseConnection (db);
					if (qtde == 0) 
					{
						//Se NÃO existe Banco de Dados
						//DisplayAlert("",AppResources.avisoNecessidadePrimeiroLoginOnline,"OK");
						Utils.ShowToast (ToastNotificationType.Warning, AppResources.avisoNecessidadePrimeiroLoginOnline);
						this.loading.IsVisible = false;
					} else {
						EstruturaTabelas.Usuario userEncontrado = resultadoConsulta.Where (u => u.Login.Equals (txbLogin.Text)).FirstOrDefault ();
						if (userEncontrado != null) 
						{
							Commons.Encryption oEncryption = new Commons.Encryption();
							string senhaBanco = oEncryption.Decrypt(userEncontrado.Senha.Replace(' ', '/'));
							if (senhaBanco.Equals (txbSenha.Text)) 
							{
								idUsuarioLogado = userEncontrado.IdUsuario;
								idClientLogado = userEncontrado.IdCliente;
								idClientHolderLogado = userEncontrado.IdHolder;
								idPerfilLogado = userEncontrado.IdPerfil;

								DBConnector.CloseConnection (db);
								var root = new SelectQuestionary();
								Navigation.PushModalAsync(root);
								this.loading.IsVisible = true;
							} 
							else 
							{
								AlertaErroLogin (AppResources.senhaIncorreta);
							}
						} 
						else 
						{
							Utils.ShowToast (ToastNotificationType.Warning, AppResources.avisoNecessidadePrimeiroLoginOnline);
							//DisplayAlert("",AppResources.avisoNecessidadePrimeiroLoginOnline,"OK");
						}
					}
				}
			}
			catch (Exception ex)
			{
				AlertaErroLogin (AppResources.erroConexao);
			}
		}


		/// <summary>
		/// Método responsável por criar todas as tabelas que a aplicação irá utilizar.
		/// </summary>
		/// <param name="db">Db.</param>
		private void CriaTabelasDaAplicacao(SQLite.Net.SQLiteConnection db)
		{
			db.CreateTable<EstruturaTabelas.Usuario> ();
			db.CreateTable<EstruturaTabelas.FotoEvidenciaPendente> ();
			db.CreateTable<EstruturaTabelas.Evidencia> ();
			db.CreateTable<EstruturaTabelas.Questionario> ();
			db.CreateTable<EstruturaTabelas.Questionario_Pergunta> ();
			db.CreateTable<EstruturaTabelas.Questionario_Alternativa> ();
			db.CreateTable<EstruturaTabelas.Questionario_Resposta> ();
			db.CreateTable<EstruturaTabelas.Questionario_Resposta_Pendente> ();
			db.CreateTable<EstruturaTabelas.Tracking> ();
			db.CreateTable<EstruturaTabelas.VozEvidenciaPendente> ();
			db.CreateTable<EstruturaTabelas.VozEvidencia> ();
			db.CreateTable<EstruturaTabelas.PlanoVoo> ();
			db.CreateTable<EstruturaTabelas.Alvo> ();
			db.CreateTable<EstruturaTabelas.AlvoVisitado> ();
			db.CreateTable<EstruturaTabelas.Configdev> ();
			db.CreateTable<EstruturaTabelas.LoginSalvo> ();
			db.CreateTable<EstruturaTabelas.Config> ();
			db.CreateTable<EstruturaTabelas.Log> ();
			//db.CreateTable<EstruturaTabelas.Descricao> ();
		}

		private bool VerificaAcessoInternet()
		{
			bool status = false;
			var networkConnection = DependencyService.Get<IVerificaAcessoInternet>();
			networkConnection.CheckNetworkConnection();
			bool networkStatus = networkConnection.IsConnected;
			if (networkStatus) 
			{
				status = true;
			}


			//DependencyService.Get<IVerificaAcessoInternet>().IsConnected ? "You are Connected" : "You are Not Connected";

			return status;


		}

		public void AlertaErroLogin (string stringMensagem)
		{
			this.lblStatusText.Text = stringMensagem;
			this.lblStatusText.IsVisible = true;
			this.loading.IsVisible = false;


		}

		void VerificaLoginSalvo()
		{

			string loginSalvo = string.Empty;
			string senhaSalva = string.Empty;
			db = DBConnector.OpenConnection ();
			//db.DeleteAll<EstruturaTabelas.Usuario> ();
			//db.DeleteAll<EstruturaTabelas.LoginSalvo> ();
			EstruturaTabelas.LoginSalvo resultadoUsuario = db.Table<EstruturaTabelas.LoginSalvo> ().FirstOrDefault();
			if (resultadoUsuario != null) 
			{
				loginSalvo = resultadoUsuario.login;
				senhaSalva = resultadoUsuario.senha;
			}


			if (!string.IsNullOrEmpty (loginSalvo) && !string.IsNullOrEmpty(senhaSalva)) 
			{ 
				this.chkSalvarDados.Checked = true;
				this.txbLogin.Text = loginSalvo;
				this.txbSenha.Text = senhaSalva;
			} 
			DBConnector.CloseConnection (db);
		}


	}
}

