using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [SerializeField] private Player _player;
    [SerializeField] private PlayerMovementConfig _playerMovementConfig;
    [SerializeField] private PlayerParameters _playerParameters;

    public override void InstallBindings()
    {
        Container.Bind<Player>().FromInstance(_player).AsSingle();

        Container.Bind<PlayerMovementConfig>().FromInstance(_playerMovementConfig).AsSingle();

        _playerParameters.Init(_player.Inventory);
        Container.Bind<PlayerParameters>().FromInstance(_playerParameters).AsSingle();
    }
}