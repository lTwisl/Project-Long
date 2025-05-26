using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [SerializeField] private World _world;

    [SerializeField] private Player _player;
    [SerializeField] private PlayerMovementConfig _playerMovementConfig;
    [SerializeField] private PlayerParameters _playerParameters;

    [SerializeField] private UseStrategy[] _useStrategies;

    public override void InstallBindings()
    {
        Container.Bind<World>().FromInstance(_world).AsSingle();

        Container.Bind<PlayerMovementConfig>().FromInstance(_playerMovementConfig).AsSingle();
        Container.Bind<PlayerParameters>().FromInstance(_playerParameters).AsSingle();
        _playerParameters.Initialize(_player.Inventory);

        Container.Bind<ClothingSystem>().AsSingle();
        Container.Bind<Inventory>().AsSingle();

        Container.Bind<Player>().FromInstance(_player).AsSingle();
    }

    private void Awake()
    {
        foreach (var useStratege in _useStrategies)
        {
            Container.Inject(useStratege);
        }
    }
}
