using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace GameOperationsSamples
{
    namespace CommandBatching
    {
        public class CommandBatchingSceneManager : MonoBehaviour
        {
            public CommandBatchingSampleView sceneView;

            async void Start()
            {
                try
                {
                    sceneView.InitializeScene();
                    await InitializeUnityServices();

                    // Check that scene has not been unloaded while processing async wait to prevent throw.
                    if (this == null) return;

                    await FetchUpdatedServicesData();
                    if (this == null) return;

                    BeginGame();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            async Task InitializeUnityServices()
            {
                await UnityServices.InitializeAsync();

                if (this == null) return;

                Debug.Log("Services Initialized.");

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("Signing in...");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    if (this == null) return;
                }

                Debug.Log($"Player id: {AuthenticationService.Instance.PlayerId}");
            }

            async Task FetchUpdatedServicesData()
            {
                await Task.WhenAll(
                    EconomyManager.instance.RefreshCurrencyBalances(),
                    RemoteConfigManager.instance.FetchConfigs(),
                    CloudSaveManager.instance.LoadAndCacheData()
                );
            }

            void BeginGame()
            {
                GameStateManager.instance.SetUpNewGame();
                sceneView.UpdateGameView();
                sceneView.BeginGame();
            }

            public async void OnDefeatRedEnemyButtonPressed()
            {
                try
                {
                    if (GameStateManager.instance.ConsumeTurnIfAnyAvailable())
                    {
                        var command = new DefeatRedEnemyCommand();
                        command.Execute();
                        sceneView.UpdateGameView();
                    }

                    await GameOverIfNecessary();
                }
                catch (Exception e)
                {
                    Debug.Log("There was a problem ending the game.");
                    Debug.LogException(e);
                }
            }

            public async void OnDefeatBlueEnemyButtonPressed()
            {
                try
                {
                    if (GameStateManager.instance.ConsumeTurnIfAnyAvailable())
                    {
                        var command = new DefeatBlueEnemyCommand();
                        command.Execute();
                        sceneView.UpdateGameView();
                    }

                    await GameOverIfNecessary();
                }
                catch (Exception e)
                {
                    Debug.Log("There was a problem ending the game.");
                    Debug.LogException(e);
                }
            }

            public async void OnOpenChestButtonPressed()
            {
                try
                {
                    if (GameStateManager.instance.ConsumeTurnIfAnyAvailable())
                    {
                        var command = new OpenChestCommand();
                        command.Execute();
                        sceneView.UpdateGameView();
                    }

                    await GameOverIfNecessary();
                }
                catch (Exception e)
                {
                    Debug.Log("There was a problem ending the game.");
                    Debug.LogException(e);
                }
            }

            public async void OnAchieveBonusGoalButtonPressed()
            {
                try
                {
                    if (GameStateManager.instance.ConsumeTurnIfAnyAvailable())
                    {
                        var command = new AchieveBonusGoalCommand();
                        command.Execute();
                        sceneView.UpdateGameView();
                    }

                    await GameOverIfNecessary();
                }
                catch (Exception e)
                {
                    Debug.Log("There was a problem ending the game.");
                    Debug.LogException(e);
                }
            }

            async Task GameOverIfNecessary()
            {
                if (!GameStateManager.instance.IsGameOver())
                {
                    return;
                }

                var command = new GameOverCommand();
                command.Execute();
                sceneView.GameOver();
                await HandleGameOver();
            }

            async Task HandleGameOver()
            {
                try
                {
                    await CommandBatchSystem.instance.FlushBatch();
                    if (this == null) return;

                    await FetchUpdatedServicesData();
                    if (this == null) return;

                    sceneView.ShowGameOverPlayAgainButton();
                }
                catch (Exception e)
                {
                    Debug.Log("There was a problem communicating with the server.");
                    Debug.LogException(e);
                }
                
            }

            public void OnGameOverPlayAgainButtonPressed()
            {
                sceneView.CloseGameOverPopup();
                BeginGame();
            }
        }
    }
}
