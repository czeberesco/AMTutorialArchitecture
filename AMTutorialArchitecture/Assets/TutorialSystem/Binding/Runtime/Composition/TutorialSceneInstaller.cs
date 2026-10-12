using AMTutorialArchitecture.Tutorial.Application.ViewModels;
using AMTutorialArchitecture.Tutorial.Unity.Integration;
using AMTutorialArchitecture.Tutorial.Unity.Targets;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Composition
{
    public sealed class TutorialSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TutorialViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<TutorialTargetRegistry>().AsSingle();
            Container.BindInterfacesAndSelfTo<TutorialActivityService>().AsSingle();
        }
    }
}
