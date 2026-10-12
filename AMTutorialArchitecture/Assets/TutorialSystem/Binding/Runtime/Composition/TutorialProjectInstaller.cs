using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Domain.Commands;
using AMTutorialArchitecture.Tutorial.Domain.Model;
using AMTutorialArchitecture.Tutorial.Domain.Validation;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Composition
{
    public sealed class TutorialProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<TutorialModel>().AsSingle();
            Container.Bind<TutorialDefinitionValidator>().AsSingle();
            Container.Bind<TutorialCommandProcessor>().AsSingle();
            Container.Bind<ITutorialService>().To<TutorialService>().AsSingle();
        }
    }
}
