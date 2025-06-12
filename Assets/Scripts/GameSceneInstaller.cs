using ClothingSystems;
using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [SerializeField] private World _world;

    [SerializeField] private Player _player;
    [SerializeField] private PlayerParameters _playerParameters;

    [SerializeField] private MovementSettings _movementSettings;
    [SerializeField] private ClothingSystemConfig _clothingSystemConfig;

    [SerializeField] private UseStrategy[] _useStrategies;

    public override void InstallBindings()
    {
        Container.Bind<MovementSettings>().FromInstance(_movementSettings).AsSingle();
        Container.Bind<ClothingSystemConfig>().FromInstance(_clothingSystemConfig).AsSingle();

        Container.Bind<World>().FromInstance(_world).AsSingle();

        Container.Bind<PlayerParameters>().FromInstance(_playerParameters).AsSingle();
        //_playerParameters.Initialize();

        //Container.Bind<Inventory>().AsSingle();

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
