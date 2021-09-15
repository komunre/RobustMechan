using Robust.Shared.GameObjects;
using Robust.Shared.Input;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Client.Input;
using Robust.Client;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Client.GameObjects;



namespace RobustMechan.Input
{
    public class InputHookupManager : EntitySystem
    {
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly BaseClient _baseClient = default!;
        [Dependency] private readonly IEyeManager _eyeManager = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
        public override void Initialize() {
            _inputManager.KeyBindStateChanged += OnKeyStateChanged;
        }

        private void OnKeyStateChanged(ViewportBoundKeyEventArgs args) {
            if (_baseClient.RunLevel != ClientRunLevel.InGame)
                return;

            if (!_entitySystemManager.TryGetEntitySystem<InputSystem>(out var inputSystem)) 
                return;

            var mouseCoord = _eyeManager.ScreenToMap(args.KeyEventArgs.PointerLocation).Position;

            var entCoord = EntityCoordinates.Invalid;
            var entityLookup = IoCManager.Resolve<IEntityLookup>();
            var entitiesUnder = entityLookup.GetEntitiesInRange(_playerManager.LocalPlayer.ControlledEntity.Transform.MapID, mouseCoord, 0.3f);
            IEntity entityUnder = null;
            foreach (var ent in entitiesUnder) {
                entityUnder = ent;
                break;
            }
            EntityUid entityUnderUid;
            if (entityUnder != null) {
                Logger.Debug("Entity detected: " + entityUnder.ToString() + ", " + ((int)entityUnder.Uid));
                entityUnderUid = entityUnder.Uid;
            }
            else {
                entityUnderUid = EntityUid.Invalid;
            }

            var message = new FullInputCmdMessage(_gameTiming.CurTick, _gameTiming.TickFraction, _inputManager.NetworkBindMap.KeyFunctionID(args.KeyEventArgs.Function), args.KeyEventArgs.State, entCoord, args.KeyEventArgs.PointerLocation, entityUnderUid);
            if (inputSystem.HandleInputCommand(_playerManager.LocalPlayer.Session, args.KeyEventArgs.Function, message)) {
                args.KeyEventArgs.Handle();
            }
        }
    }
}