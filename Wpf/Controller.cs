using System;
using System.Windows.Input;

namespace Wpf
{
	public class Controller
	{
		private readonly MainWindow mainWindow;
		private readonly HistoryCommand historyCommand;
		private readonly CmdReader cmdReader;
		private readonly TabHandler tabHandler;
		private readonly Terminal terminal;

		public Controller(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;

			historyCommand = new HistoryCommand();
			cmdReader = new CmdReader(this);
			terminal = new Terminal(mainWindow.Rst);
			tabHandler = new TabHandler(terminal);
		}

		public void Init()
		{
			cmdReader.Init();
		}

		public void AddData(string outputs)
		{
			tabHandler.ExtractDir(ref outputs);
			tabHandler.ResetTabComplete();

			Action act = () =>
			{
				terminal.AppendOutput(outputs);
			};

			mainWindow.Dispatcher.BeginInvoke(act);
		}

		private void RunCmd()
		{
			string cmd = terminal.GetCmd();

			if (cmd == "cls")
			{
				Action act = () =>
				{
					terminal.Clear();
				};

				mainWindow.Dispatcher.Invoke(act);

				cmdReader.Input("");
			}
			else
			{
				Action act = () =>
				{
					terminal.removeInput();
				};

				mainWindow.Dispatcher.Invoke(act); //No async, ensure is done

				cmdReader.Input(cmd);
			}

			historyCommand.Add(cmd);
		}

		public void HandleInput(KeyEventArgs e)
		{
			if (terminal.CaretIndex < terminal.DataLen)
			{
				if (e.Key != Key.Left && e.Key != Key.Right)
				{
					e.Handled = true;
				}

				return;
			}

			if (e.Key == Key.Back && terminal.CaretIndex <= terminal.DataLen)
			{
				e.Handled = true;
				return;
			}

			if (e.Key == Key.Tab)
			{
				e.Handled = true;

				if (tabHandler.HandleTab())
				{
					return;
				}
			}

			tabHandler.ResetTabComplete();

			if (e.Key == Key.Up)
			{
				terminal.setInput(historyCommand.SelectPreviuos());

				e.Handled = true;
			}
			else if (e.Key == Key.Down)
			{

				terminal.setInput(historyCommand.SelectNext());

				e.Handled = true;
			}
			else if (e.Key == Key.C && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)) //Keyboard.IsKeyDown(Key.LeftCtrl)
			{
				cmdReader.SendCtrlC();

				e.Handled = true;
			}
			else if (e.Key == Key.Return)
			{
				RunCmd();

				e.Handled = true;
			}
		}

		public void Close()
		{
			cmdReader.Close();
		}

		public void ClearOutput()
		{
			terminal.Clear();
			cmdReader.Input("");
		}

		public void RestartProc()
		{
			terminal.Clear();
			cmdReader.Restart();
		}
	}
}