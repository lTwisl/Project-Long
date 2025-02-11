using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [SerializeField] private PlayerMovementConfig _playerMovementConfig;

    public override void InstallBindings()
    {
        Container.Bind<PlayerMovementConfig>().FromInstance(_playerMovementConfig).AsSingle();
    }
}