﻿/* -LICENSE-START-
** Copyright (c) 2019 Blackmagic Design
**
** Permission is hereby granted, free of charge, to any person or organization
** obtaining a copy of the software and accompanying documentation covered by
** this license (the "Software") to use, reproduce, display, distribute,
** execute, and transmit the Software, and to prepare derivative works of the
** Software, and to permit third-parties to whom the Software is furnished to
** do so, all subject to the following:
** 
** The copyright notices in the Software and this entire statement, including
** the above license grant, this restriction and the following disclaimer,
** must be included in all copies of the Software, in whole or in part, and
** all derivative works of the Software, unless such copies or derivative
** works are solely in the form of machine-executable object code generated by
** a source language processor.
** 
** THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
** IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
** FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT
** SHALL THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE
** FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE,
** ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
** DEALINGS IN THE SOFTWARE.
** -LICENSE-END-
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using BMDSwitcherAPI;
using AutoMapper;

namespace SimpleSwitcher
{
	public class LumaParameters
	{
		public int inputFill = 0;
		public int inputKey = 0;

		public int masked = 0;
		public double maskTop = 0;
		public double maskBottom = 0;
		public double maskRight = 0;
		public double maskLeft = 0;

		public int preMultiplied = 0;
		public double preMultipliedClip = 0;
		public double preMultipliedGain = 0;
		public int preMultipliedInvertKey = 0;

		public int fly = 1;
		public double flySizeX = 1;
		public double flySizeY = 1;
		public double flyPositionX = 0;
		public double flyPositionY = 13.9;
	}

	public class RunCommands
	{
		private int tempoTransicaoPadrao = 20;
		private AtemSwitcher atem = null;
		IBMDSwitcherMixEffectBlock me0 = null;

		/*
		public static void Main(string[] args)
		{
			// Display the number of command line arguments.
			Console.WriteLine("Teste");
			RunCommands run = new RunCommands();
			run.ConectarSwitcher("192.168.0.100");
		}
		*/

		public bool ConectarSwitcher(string ip)
		{
			int tentativa = 0;
			int maxTentativa = 3;
			_BMDSwitcherConnectToFailure failReason = 0;
			bool conectado = false;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::ConectarSwitcher");

			while (tentativa < maxTentativa)
			{
				tentativa++;
				try
				{
					Console.WriteLine("Tentativa de conexão (" + tentativa + ")");

					// Create switcher discovery object
					IBMDSwitcherDiscovery discovery = new CBMDSwitcherDiscovery();

					// Connect to switcher
					discovery.ConnectTo(ip, out IBMDSwitcher switcher, out failReason);
					Console.WriteLine("Connectado ao switcher");

					atem = new AtemSwitcher(switcher);
					me0 = atem.MixEffectBlocks.First();

					conectado = true;
					break;
				}
				catch (COMException e1)
				{
					// An exception will be thrown if ConnectTo fails.
					if (failReason == _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureNoResponse)
					{
						System.Threading.Thread.Sleep(3000);
					}
					else if (failReason == _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureIncompatibleFirmware)
					{
						Console.WriteLine("Switcher tem um firmware incompatível");
						break;
					}
					else 
					{ 
						Console.WriteLine("Conexão falhou por um erro desconhecido");
						Console.WriteLine("Mensagem: " + e1.Message);
						Console.WriteLine(e1.StackTrace);
						break;
					}
				}
				catch (Exception e2)
                {
					Console.WriteLine("Ocorreu uma exceção no sistema");
					Console.WriteLine("Mensagem: " + e2.Message);
					Console.WriteLine(e2.StackTrace);
					break;
				}
			}

			return conectado;
		}

		public void RunTest(string ip, int destInput, int transitionRate)
		{
			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::RunTest");
			if(!ConectarSwitcher(ip)) return;

			// Get reference to various objects

			IBMDSwitcherTransitionParameters me0TransitionParams = me0 as IBMDSwitcherTransitionParameters;
			IBMDSwitcherTransitionWipeParameters me0WipeTransitionParams = me0 as IBMDSwitcherTransitionWipeParameters;
			
			// Setup the transition
			Console.WriteLine("Setting preview input");
			SetProgramPreview(false, "external", destInput);
			//IBMDSwitcherInput switcherInput = atem.SwitcherInputsTypeExternal.ElementAt(destInput);
			//me0.SetPreviewInput(GetInputId(switcherInput));

			Console.WriteLine("Setting next transition selection");
			me0TransitionParams.SetNextTransitionSelection(_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground);

			Console.WriteLine("Setting next transition style");
			me0TransitionParams.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleWipe);

			Console.WriteLine("Setting transition style");
			me0WipeTransitionParams.SetPattern(_BMDSwitcherPatternStyle.bmdSwitcherPatternStyleRectangleIris);

			Console.WriteLine("Setting transition rate");
			me0WipeTransitionParams.SetRate((uint)transitionRate);
			System.Threading.Thread.Sleep(100);

			// Perform the transition
			Console.WriteLine("Performing auto transition I");
			me0.PerformAutoTransition();
			System.Threading.Thread.Sleep(transitionRate * (1 / 30) * 1000);

			// Espera um pouquinho
			System.Threading.Thread.Sleep(1000);

			Console.WriteLine("Performing auto transition II");
			me0.PerformAutoTransition();
			System.Threading.Thread.Sleep(transitionRate * (1 / 30) * 1000);

		}

		public void SwitcherStatus(string ip)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::SwitcherStatus");

			// Get reference to various objects
			Console.WriteLine("");
			Console.WriteLine("-------------------------------------------------------");
			Console.WriteLine("GetVideoMode ...: " + atem.GetVideoMode);
			Console.WriteLine("GetProductName .: " + atem.GetProductName);
			Console.WriteLine("-------------------------------------------------------");
			Console.WriteLine("");

		}

		public void CarregarImagemTemaCulto(string ip, int imagemIndex)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::CarregarImagemTemaCulto");

			//Carregar imagem do tema do culto

			//- Preparar inicio do culto (carregar imagem tema em MP2)
			Console.WriteLine("Carregando imagem no MP2");
			IBMDSwitcherMediaPlayer mediaPlayer = atem.MediaPlayerInputs.ElementAt(1);
			mediaPlayer.SetSource(_BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeStill, (uint)imagemIndex);

			//- Desligar DSK 1 e 2
			Console.WriteLine("Desativando DSK 1");
			atem.DownstreamInputs.ElementAt(0).SetOnAir(0);
			System.Threading.Thread.Sleep(100);

			Console.WriteLine("Desativando DSK 2");
			atem.DownstreamInputs.ElementAt(1).SetOnAir(0);
			System.Threading.Thread.Sleep(100);

			//- Colocar imagem MP2 no PGM
			Console.WriteLine("Ativando MP2 como input no PGM");
			SetProgramPreview(true, "media", 1);
			//IBMDSwitcherInput switcherInput = atem.SwitcherInputsTypeMediaPlayerFill.ElementAt(1);
			//me0.SetProgramInput(GetInputId(switcherInput));
		}

		public void ExecutarAberturaCulto(string ip, int tempoTransicao)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::ExecutarAberturaCulto");

			IBMDSwitcherTransitionParameters me0TransitionParams = me0 as IBMDSwitcherTransitionParameters;
			IBMDSwitcherTransitionMixParameters me0MixTransitionParams = me0 as IBMDSwitcherTransitionMixParameters;

			//Abertura do culto
			//- Colocar imagem MP2 no PGM
			Console.WriteLine("Ativando MP2 como input no PGM");
			SetProgramPreview(true, "media", 1);
			//IBMDSwitcherInput switcherInput = atem.SwitcherInputsTypeMediaPlayerFill.ElementAt(1);
			//me0.SetProgramInput(GetInputId(switcherInput));

			//- Setar transicao para 60
			Console.WriteLine("Definindo transição Mix para 60 FPS");

			me0TransitionParams.SetNextTransitionSelection(_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground);
			me0TransitionParams.SetNextTransitionStyle(_BMDSwitcherTransitionStyle.bmdSwitcherTransitionStyleMix);
			me0MixTransitionParams.SetRate((uint)tempoTransicao);
			System.Threading.Thread.Sleep(100);

			//- Fade para imagem que estiver no preview
			me0.PerformAutoTransition();
			System.Threading.Thread.Sleep(tempoTransicao * (1 / 30) * 1000);
			System.Threading.Thread.Sleep(100);

			//- Setar transicao para 20
			me0MixTransitionParams.SetRate((uint)tempoTransicaoPadrao);
			System.Threading.Thread.Sleep(100);
		}

		public void ExibirOferta(string ip, int imagemIndex)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::ExibirOferta");

			//Exibir Oferta
			//- Verifica se não está no ar
			Console.WriteLine("Verifica se não está no ar");
			IBMDSwitcherDownstreamKey downstream = atem.DownstreamInputs.ElementAt(0);
			downstream.GetOnAir(out int onair);
			if (onair == 1)
			{
				Console.WriteLine("DSK 1 já está em exibição");
				return;
			}

			//- Carregar imagem da oferta em MP1
			Console.WriteLine("Carregando imagem da oferta em MP1");
			IBMDSwitcherMediaPlayer mediaPlayer = atem.MediaPlayerInputs.ElementAt(0);
			mediaPlayer.SetSource(_BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeStill, (uint)imagemIndex);

			//- Setar transicao para 15 em DSK1
			Console.WriteLine("Defindo velocidade para 15 FPS");
			downstream.SetRate(15);
			System.Threading.Thread.Sleep(100);

			//- Fazer o fade do DSK1
			Console.WriteLine("Exibindo DSK 1");
			downstream.PerformAutoTransition();
			System.Threading.Thread.Sleep(15 * (1 / 30) * 1000);

			System.Threading.Thread.Sleep(100);
		}

		public void OcultarOferta(string ip)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::OcultarOferta");

			//Ocultar Oferta
			//- Verifica se não está no ar
			Console.WriteLine("Verifica se está no ar");
			IBMDSwitcherDownstreamKey downstream = atem.DownstreamInputs.ElementAt(0);
			downstream.GetOnAir(out int onair);
			if (onair == 0)
			{
				Console.WriteLine("DSK 1 não está em exibição");
				return;
			}

			//- Fazer o fade do DSK1
			Console.WriteLine("Ocultando DSK 1");
			downstream.PerformAutoTransition();
			System.Threading.Thread.Sleep(15 * (1 / 30) * 1000);

			System.Threading.Thread.Sleep(100);
		}

		public void ExecutarEncerramentoCulto(string ip, int imagemIndexEncerramento, int inputEncerramento, int tempoTransicao, int tempoEsperaAntesDeEncerrar)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::ExecutarEncerramentoCulto");

			IBMDSwitcherTransitionParameters me0TransitionParams = me0 as IBMDSwitcherTransitionParameters;
			IBMDSwitcherTransitionMixParameters me0MixTransitionParams = me0 as IBMDSwitcherTransitionMixParameters;

			//Encerramento do Culto
			//- Colocar imagem de encerramento em MP2
			Console.WriteLine("Carregando imagem no MP2");
			IBMDSwitcherMediaPlayer mediaPlayer = atem.MediaPlayerInputs.ElementAt(1);
			mediaPlayer.SetSource(_BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeStill, (uint)imagemIndexEncerramento);

			//- Colocar PTZ7 no Preview (Input 7)
			Console.WriteLine("Colocar PTZ7 no Preview (Input 7)");
			SetProgramPreview(false, "external", inputEncerramento);
			//IBMDSwitcherInput switcherInput1 = atem.SwitcherInputsTypeExternal.ElementAt(inputEncerramento);
			//me0.SetPreviewInput(GetInputId(switcherInput1));
			System.Threading.Thread.Sleep(1000);

			//- Setar transicao para 20
			Console.WriteLine("Setar transicao para 20");
			me0MixTransitionParams.SetRate((uint)tempoTransicaoPadrao);
			System.Threading.Thread.Sleep(100);

			//- Fade para Preview
			Console.WriteLine("Fade para Preview");
			me0.PerformAutoTransition();
			System.Threading.Thread.Sleep(tempoTransicaoPadrao * (1 / 30) * 1000);
			System.Threading.Thread.Sleep(100);

			// Esperar um tempo
			Console.WriteLine("Esperar um tempo");
			System.Threading.Thread.Sleep(tempoEsperaAntesDeEncerrar);
			System.Threading.Thread.Sleep(100);

			//- Colocar MP2 no Preview
			Console.WriteLine("Colocar MP2 no Preview");
			SetProgramPreview(false, "media", 1);
			//IBMDSwitcherInput switcherInput2 = atem.SwitcherInputsTypeMediaPlayerFill.ElementAt(1);
			//me0.SetPreviewInput(GetInputId(switcherInput2));
			System.Threading.Thread.Sleep(100);

			//- Setar transicao para 40
			Console.WriteLine("Setar transicao para 40");
			me0MixTransitionParams.SetRate((uint)tempoTransicao);
			System.Threading.Thread.Sleep(100);

			//- Fade para Preview
			Console.WriteLine("Fade para Preview");
			me0.PerformAutoTransition();
			System.Threading.Thread.Sleep(tempoTransicao * (1 / 30) * 1000);
			System.Threading.Thread.Sleep(100);

			//- Setar transicao para 20
			Console.WriteLine("Setar transicao para 20");
			me0MixTransitionParams.SetRate((uint)tempoTransicaoPadrao);
			System.Threading.Thread.Sleep(100);
		}

		public void AtivarLegendaCoral(string ip, dynamic dynamicLumaParameters)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::AtivarLegendaCoral");

			//- Setar PC em Chave Upstream 1
			SetUpstream1(dynamicLumaParameters);

			//- Ativar KEY1
			Console.WriteLine("Ativando KEY 1");
			IBMDSwitcherTransitionParameters me0TransitionParams = me0 as IBMDSwitcherTransitionParameters;
			me0TransitionParams.SetNextTransitionSelection(_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1);

			//- Ativar CUT
			// Não localizei essa opção no SDK
			// Vai ter que fazer na mão mesmo

			//- Setar Auxiliar 1 para Preview
			SetAuxiliaryOutput("output", 3);
		}

		public void DesativarLegendaCoral(string ip)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::DesativarLegendaCoral");

			IBMDSwitcherTransitionParameters me0TransitionParams = me0 as IBMDSwitcherTransitionParameters;
			
			//Desativar Legenda Coral
			//- Setar Auxiliar 1 para PGM
			SetAuxiliaryOutput("output", 2);

			//- Ativar BKGD
			Console.WriteLine("Ativando BKGD");
			me0TransitionParams.SetNextTransitionSelection(_BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground);

			//- Desativar CUT
			// Não localizei essa opção no SDK
			// Vai ter que fazer na mão mesmo
		}

		public void AtivarUpstream1(string ip, dynamic dynamicLumaParameters)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::AtivarUpstream1");

			//- Setar Chave Upstream 1
			SetUpstream1(dynamicLumaParameters);
		}

		public void DefinirSaidaAuxiliar(string ip, string tipoInput, int inputIndex)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::DefinirSaidaAuxiliar");

			//DefinirSaidaAuxiliar
			//- Setar Auxiliar 1 para a recebida por parametro
			SetAuxiliaryOutput(tipoInput, inputIndex);
		}

		public void DefinirPreview(string ip, string tipoInput, int inputIndex)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::DefinirPreview");
			SetProgramPreview(false, tipoInput, inputIndex);
		}

		public void DefinirProgram(string ip, string tipoInput, int inputIndex)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::DefinirProgram");
			SetProgramPreview(true, tipoInput, inputIndex);
		}

		public void PerformAutoTransition(string ip)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::PerformAutoTransition");
			me0.PerformAutoTransition();
		}

		public void ListarSwitcherInputs(string ip)
		{
			if (!ConectarSwitcher(ip)) return;

			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::ListarSwitcherInputs");

			int i = 0;

			Console.WriteLine("Carregando objeto auxOutput");
			atem.SwitcherInputs.ToList().ForEach((e) =>
			{
				e.GetPortType(out _BMDSwitcherPortType type);
				Console.WriteLine("[" + (i++) + "] " + type);
			});

		}

		private long GetInputId(IBMDSwitcherInput input)
		{
			input.GetInputId(out long id);
			return id;
		}

		public void SetUpstream1(dynamic dynamicLumaParameters)
		{
			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("::SetUpstream1");

			var configuration = new MapperConfiguration(cfg => { });
			var mapper = new Mapper(configuration);

			LumaParameters lumaParameters = mapper.Map<LumaParameters>(dynamicLumaParameters);

			//Ativar Legenda Coral
			//- Setar PC em Chave Upstream 1
			Console.WriteLine("Definindo PC em Chave Upstream 1");
			IBMDSwitcherKey key = atem.SwitcherKey.ElementAt(0);
			key.SetInputFill(GetInputId(atem.SwitcherInputsTypeExternal.ElementAt(lumaParameters.inputFill)));
			key.SetInputCut(GetInputId(atem.SwitcherInputsTypeExternal.ElementAt(lumaParameters.inputKey)));

			key.SetMasked(lumaParameters.masked);
			key.SetMaskTop(lumaParameters.maskTop);
			key.SetMaskBottom(lumaParameters.maskBottom);
			key.SetMaskLeft(lumaParameters.maskLeft);
			key.SetMaskRight(lumaParameters.maskRight);

			IBMDSwitcherKeyLumaParameters keyLumaParameters = key as IBMDSwitcherKeyLumaParameters;
			keyLumaParameters.SetPreMultiplied(lumaParameters.preMultiplied);
			keyLumaParameters.SetClip((double)lumaParameters.preMultipliedClip);
			keyLumaParameters.SetGain((double)lumaParameters.preMultipliedGain);
			keyLumaParameters.SetInverse(lumaParameters.preMultipliedInvertKey);

			//- Configurar posição
			IBMDSwitcherKeyFlyParameters keyFlyParameters = key as IBMDSwitcherKeyFlyParameters;
			keyFlyParameters.SetFly(lumaParameters.fly);
			keyFlyParameters.SetSizeX(lumaParameters.flySizeX);
			keyFlyParameters.SetSizeY(lumaParameters.flySizeY);
			keyFlyParameters.SetPositionX(lumaParameters.flyPositionX);
			keyFlyParameters.SetPositionY(lumaParameters.flyPositionY);

		}

		private void SetProgramPreview(bool ehPrograma, string tipoInput, int inputIndex)
		{
			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("PGM/Preview :: Carregando saida");
			IBMDSwitcherInput switcherInput = null;

			switch (tipoInput)
			{
				case "external":
					// 0 - Cam1
					// 1 - Cam2
					// ...
					// 7 - Cam8
					Console.WriteLine("AUX :: Buscando saida Externa");
					switcherInput = atem.SwitcherInputsTypeExternal.ElementAt(inputIndex);
					break;
				case "media":
					// 0 - MP1
					// 1 - MP2
					Console.WriteLine("AUX :: Buscando saida Media Player");
					switcherInput = atem.SwitcherInputsTypeMediaPlayerFill.ElementAt(inputIndex);
					break;
				case "output":
					// 0 - Clean Feed 1
					// 1 - Clean Feed 2
					// 2 - Program (PGM)
					// 3 - Preview
					Console.WriteLine("AUX :: Buscando saida Output");
					switcherInput = atem.SwitcherInputsTypeMixEffectBlockOutput.ElementAt(inputIndex);
					break;
				default:
					throw new Exception("Tipo [" + tipoInput + "] não identificado");
					break;
			}

			if(ehPrograma)
            {
				Console.WriteLine("PGM/Preview :: Definindo saida PGM");
				me0.SetProgramInput(GetInputId(switcherInput));
			}
			else
            {
				Console.WriteLine("PGM/Preview :: Definindo saida Preview");
				me0.SetPreviewInput(GetInputId(switcherInput));
			}

		}

		private void SetAuxiliaryOutput(string tipoInput, int inputIndex)
		{
			Console.WriteLine("");
			Console.WriteLine("----------------------------------");
			Console.WriteLine("AUX :: Carregando objeto auxOutput");
			IBMDSwitcherInputAux auxOutput = atem.SwitcherInputsTypeAuxOutput.ElementAt(0) as IBMDSwitcherInputAux;

			IBMDSwitcherInput switcherInput = null;

			switch (tipoInput)
			{
				case "external":
					// 0 - Cam1
					// 1 - Cam2
					// ...
					// 7 - Cam8
					Console.WriteLine("AUX :: Buscando saida Externa");
					switcherInput = atem.SwitcherInputsTypeExternal.ElementAt(inputIndex);
					break;
				case "media":
					// 0 - MP1
					// 1 - MP2
					Console.WriteLine("AUX :: Buscando saida Media Player");
					switcherInput = atem.SwitcherInputsTypeMediaPlayerFill.ElementAt(inputIndex);
					break;
				case "output":
					// 0 - Clean Feed 1
					// 1 - Clean Feed 2
					// 2 - Program (PGM)
					// 3 - Preview
					Console.WriteLine("AUX :: Buscando saida Output");
					switcherInput = atem.SwitcherInputsTypeMixEffectBlockOutput.ElementAt(inputIndex);
					break;
				default:
					throw new Exception("Tipo [" + tipoInput + "] não identificado");
					break;
			}

			Console.WriteLine("AUX :: Definindo saida no auxiliar");
			auxOutput.SetInputSource(GetInputId(switcherInput));
		}

	}

	internal class AtemSwitcher
	{
		private IBMDSwitcher switcher;

		public AtemSwitcher(IBMDSwitcher switcher) => this.switcher = switcher;

		public IEnumerable<IBMDSwitcherMixEffectBlock> MixEffectBlocks
		{
			get
			{
				// Create a mix effect block iterator
				switcher.CreateIterator(typeof(IBMDSwitcherMixEffectBlockIterator).GUID, out IntPtr meIteratorPtr);
				IBMDSwitcherMixEffectBlockIterator meIterator = Marshal.GetObjectForIUnknown(meIteratorPtr) as IBMDSwitcherMixEffectBlockIterator;
				if (meIterator == null)
					yield break;

				// Iterate through all mix effect blocks
				while (true)
				{
					meIterator.Next(out IBMDSwitcherMixEffectBlock me);

					if (me != null)
						yield return me;
					else
						yield break;
				}
			}
		}

		public IEnumerable<IBMDSwitcherInput> SwitcherInputs
		{
			get
			{
				// Create an input iterator
				switcher.CreateIterator(typeof(IBMDSwitcherInputIterator).GUID, out IntPtr inputIteratorPtr);
				IBMDSwitcherInputIterator inputIterator = Marshal.GetObjectForIUnknown(inputIteratorPtr) as IBMDSwitcherInputIterator;
				if (inputIterator == null)
					yield break;

				// Scan through all inputs
				while (true)
				{
					inputIterator.Next(out IBMDSwitcherInput input);

					if (input != null)
						yield return input;
					else
						yield break;
				}
			}
		}
		public IEnumerable<IBMDSwitcherInput> SwitcherInputsTypeExternal
		{
			get
			{
				return this.SwitcherInputs
						.Where((i, ret) =>
						{
							i.GetPortType(out _BMDSwitcherPortType type);
							return type == _BMDSwitcherPortType.bmdSwitcherPortTypeExternal;
						});
			}
		}

		public IEnumerable<IBMDSwitcherInput> SwitcherInputsTypeMediaPlayerFill
        {
			get
            {
				return this.SwitcherInputs
						.Where((i, ret) =>
						{
							i.GetPortType(out _BMDSwitcherPortType type);
							return type == _BMDSwitcherPortType.bmdSwitcherPortTypeMediaPlayerFill;
						});
			}
        }

		public IEnumerable<IBMDSwitcherInput> SwitcherInputsTypeAuxOutput
		{
			get
			{
				return this.SwitcherInputs
						.Where((i, ret) =>
						{
							i.GetPortType(out _BMDSwitcherPortType type);
							return type == _BMDSwitcherPortType.bmdSwitcherPortTypeAuxOutput;
						});
			}
		}

		public IEnumerable<IBMDSwitcherInput> SwitcherInputsTypeMixEffectBlockOutput
		{
			get
			{
				return this.SwitcherInputs
						.Where((i, ret) =>
						{
							i.GetPortType(out _BMDSwitcherPortType type);
							return type == _BMDSwitcherPortType.bmdSwitcherPortTypeMixEffectBlockOutput;
						});
			}
		}	

		public IEnumerable<IBMDSwitcherMediaPlayer> MediaPlayerInputs
		{
			get
			{
				// Create an input iterator
				switcher.CreateIterator(typeof(IBMDSwitcherMediaPlayerIterator).GUID, out IntPtr inputIteratorPtr);
				IBMDSwitcherMediaPlayerIterator inputIterator = Marshal.GetObjectForIUnknown(inputIteratorPtr) as IBMDSwitcherMediaPlayerIterator;
				if (inputIterator == null)
					yield break;

				// Scan through all inputs
				while (true)
				{
					inputIterator.Next(out IBMDSwitcherMediaPlayer input);

					if (input != null)
						yield return input;
					else
						yield break;
				}
			}
		}

		public IEnumerable<IBMDSwitcherDownstreamKey> DownstreamInputs
		{
			get
			{
				// Create an input iterator
				switcher.CreateIterator(typeof(IBMDSwitcherDownstreamKeyIterator).GUID, out IntPtr inputIteratorPtr);
				IBMDSwitcherDownstreamKeyIterator inputIterator = Marshal.GetObjectForIUnknown(inputIteratorPtr) as IBMDSwitcherDownstreamKeyIterator;
				if (inputIterator == null)
					yield break;

				// Scan through all inputs
				while (true)
				{
					inputIterator.Next(out IBMDSwitcherDownstreamKey input);

					if (input != null)
						yield return input;
					else
						yield break;
				}
			}
		}

		public IEnumerable<IBMDSwitcherKey> SwitcherKey
		{
			get
			{
				// Create an input iterator
				this.MixEffectBlocks.ElementAt(0).CreateIterator(typeof(IBMDSwitcherKeyIterator).GUID, out IntPtr inputIteratorPtr);
				IBMDSwitcherKeyIterator inputIterator = Marshal.GetObjectForIUnknown(inputIteratorPtr) as IBMDSwitcherKeyIterator;
				if (inputIterator == null)
					yield break;

				// Scan through all inputs
				while (true)
				{
					inputIterator.Next(out IBMDSwitcherKey input);

					if (input != null)
						yield return input;
					else
						yield break;
				}
			}
		}

		public _BMDSwitcherVideoMode GetVideoMode
		{
			get
			{
				// Create an input iterator
				switcher.GetVideoMode(out _BMDSwitcherVideoMode videoMode);
				return videoMode;
			}
		}

		public string GetProductName
		{
			get
			{
				// Create an input iterator
				switcher.GetProductName(out string productName);
				return productName;
			}
		}

	}
}
