namespace Savor22b.Tests.Action;

using System;
using System.Collections.Immutable;
using Libplanet;
using Libplanet.State;
using Savor22b.Action;
using Savor22b.States;
using Xunit;

public class UseLifeStoneActionTests : ActionTests
{
    private static readonly int LifeStoneItemId = 2;

    [Fact]
    public void UseLifeStoneAction_Success()
    {
        var stateDelta = CreatePresetStateDelta();

        var food = DeriveRootStateFromAccountStateDelta(stateDelta)
            .InventoryState
            .RefrigeratorStateList[0];

        var action = new UseLifeStoneAction(food.StateID);

        stateDelta = action.Execute(
            new DummyActionContext
            {
                PreviousStates = stateDelta,
                Signer = SignerAddress(),
                Random = random,
                Rehearsal = false,
                BlockIndex = 1,
            }
        );

        food.IsSuperFood = true;

        var newInventoryState = DeriveRootStateFromAccountStateDelta(stateDelta).InventoryState;

        Assert.Equal(food, newInventoryState.RefrigeratorStateList[0]);
        Assert.Empty(newInventoryState.ItemStateList);
    }

    [Fact]
    public void UseLifeStoneAction_AssertPresetStateDelta()
    {
        var stateDelta = CreatePresetStateDelta();
        var inventoryState = DeriveRootStateFromAccountStateDelta(stateDelta).InventoryState;

        Assert.Single(inventoryState.ItemStateList);
        Assert.Equal(LifeStoneItemId, inventoryState.ItemStateList[0].ItemID);
        Assert.Single(inventoryState.RefrigeratorStateList);
        Assert.False(inventoryState.RefrigeratorStateList[0].IsSuperFood);
    }

    private IAccountStateDelta CreatePresetStateDelta()
    {
        IAccountStateDelta state = new DummyState();
        Address signerAddress = SignerAddress();

        var rootStateEncoded = state.GetState(signerAddress);
        RootState rootState = rootStateEncoded is Bencodex.Types.Dictionary bdict
            ? new RootState(bdict)
            : new RootState();

        InventoryState inventoryState = rootState.InventoryState;

        inventoryState = inventoryState.AddItem(new ItemState(Guid.NewGuid(), LifeStoneItemId));

        var food = RefrigeratorState.CreateFood(
            Guid.NewGuid(),
            1,
            "D",
            1,
            1,
            1,
            1,
            1,
            ImmutableList<Guid>.Empty
        );
        inventoryState = inventoryState.AddRefrigeratorItem(food);

        rootState.SetInventoryState(inventoryState);

        return state.SetState(signerAddress, rootState.Serialize());
    }

    private RootState DeriveRootStateFromAccountStateDelta(IAccountStateDelta stateDelta) {
        var rootStateEncoded = stateDelta.GetState(SignerAddress());
        RootState rootState = rootStateEncoded is Bencodex.Types.Dictionary bdict
            ? new RootState(bdict)
            : throw new Exception();
        return rootState;
    }
}
