using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [SerializeField] private PlayerMovementConfig _playerMovementConfig;
    [SerializeField] private PlayerParameters _playerParameters;
    //[SerializeField] private PlayerMovement _playerMovement;

    public override void InstallBindings()
    {
        Container.Bind<PlayerMovementConfig>().FromInstance(_playerMovementConfig).AsSingle();

        _playerParameters.Init();
        Container.Bind<PlayerParameters>().FromInstance(_playerParameters).AsSingle();

        //Container.Bind<PlayerMovement>().FromInstance(_playerMovement).AsSingle();
    }
}