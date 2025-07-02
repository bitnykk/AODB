using AODB.Common.DbClasses;
using AODB.Common.Enums;
using AODB.Common.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AODB.Common.RDBObjects
{
    public abstract class ModifierBase
    {
        private Dictionary<FunctionType, List<Dictionary<FunctionOperator, object>>> _modifiers = new Dictionary<FunctionType, List<Dictionary<FunctionOperator, object>>>();

        public void AddModifier(FunctionType function, Dictionary<FunctionOperator, object> operations)
        {
            if (!_modifiers.ContainsKey(function))
                _modifiers.Add(function, new List<Dictionary<FunctionOperator, object>>());

            _modifiers[function].Add(operations);
        }
    }

    public class OnUse : ModifierBase
    {

    }

    public class OnActivate : ModifierBase
    {

    }

    public class OnEnter : ModifierBase
    {

    }

    public class OnWear : ModifierBase
    {

    }

    public class Requirement
    {
        private Dictionary<ActionType, List<RequirementCriterion>> _criterion = new Dictionary<ActionType, List<RequirementCriterion>>();

        public void AddCriterion(ActionType type, RequirementCriterion criteria)
        {
            if (!_criterion.TryGetValue(type, out var reqs))
            {
                reqs = new List<RequirementCriterion>();
                _criterion.Add(type, reqs);

            }

            reqs.Add(criteria);
        }
    }

    [RDBRecord(RecordTypeID = 1000020)]
    public class ItemObject : RDBObject
    {
        public int DynelType;
        public string Name;
        public string Description;

        public Dictionary<StatId, uint> Stats = new Dictionary<StatId, uint>();
        public Dictionary<StatId, uint> AttackSkills = new Dictionary<StatId, uint>();
        public Dictionary<StatId, uint> DefenseSkills = new Dictionary<StatId, uint>();
        public Dictionary<int, uint> UnkSkills01 = new Dictionary<int, uint>();
        public OnUse OnUse = new OnUse();
        public OnActivate OnActivate = new OnActivate();
        public OnEnter OnEnter = new OnEnter();
        public OnWear OnWear = new OnWear();
        public Requirement Requirement = new Requirement();

        public override void Deserialize(BinaryReader reader)
        {
            DynelType = reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            ReadKeyValues(reader);
        }

        public void ReadKeyValues(BinaryReader reader)
        {
            try
            {
                while (true)
                {
                    ItemKeyIdentifier key = (ItemKeyIdentifier)reader.ReadInt32();

                    switch (key)
                    {
                        case ItemKeyIdentifier.Modifiers:       //0x2
                            ReadModifiers(reader);
                            break;
                        case ItemKeyIdentifier.Skills:          //0x4
                            ReadSkills(reader);
                            break;
                        case (ItemKeyIdentifier)0x6:            //0x6
                            ReadUnk0x6(reader);
                            break;
                        case (ItemKeyIdentifier)0x0E:           //0x0E
                            ReadUnk0xE(reader);
                            break;
                        case (ItemKeyIdentifier)0x14:           //0x14
                            ReadUnk0x14(reader);
                            break;
                        case ItemKeyIdentifier.Text:            //0x15
                            ReadText(reader);
                            break;
                        case ItemKeyIdentifier.Requirements:    //0x16
                            ReadRequirements(reader);
                            break;
                        case ItemKeyIdentifier.Stats:           //0x17
                            ReadStats(reader);
                            break;
                        default:
                            Console.WriteLine($"Unhandled key '{key}' (ReadKeyValues)");
                            Console.WriteLine(reader.ReadInt32());
                            Console.ReadLine();
                            return;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                //Console.WriteLine("All items parsed!");
            }
        }

        private void ReadUnk0xE(BinaryReader reader)
        {
            var key = reader.ReadInt32();

            switch (key)
            {
                case 0x13:
                    var count = reader.Read3F1();
                    for (int i = 0; i < count; i++)
                        Read0xEKey0x13(reader);
                    break;
                default:
                    Console.WriteLine($"Unhandled key '{key}' (ReadUnk0xE)");
                    Console.ReadLine();
                    break;
            }
        }

        private void Read0xEKey0x13(BinaryReader reader)
        {
            var key = reader.ReadInt32();
            var count = reader.Read3F1();

            switch (key)
            {
                case 0x0:
                    for (int i = 0; i < count; i++)
                        Read0xEKey0x13Key0x0(reader);
                    break;
                case 0x3:
                    for (int i = 0; i < count; i++)
                        Read0xEKey0x13Key0x3(reader);
                    break;
                case 0x5:
                    for (int i = 0; i < count; i++)
                        Read0xEKey0x13Key0x5(reader);
                    break;
                case 0xB:
                    for (int i = 0; i < count; i++)
                        Read0xEKey0x13Key0xB(reader);
                    break;
                case 0x2:
                    for (int i = 0; i < count; i++)
                        Read0xEKey0x13Key0x2(reader);
                    break;
                case 0x8:
                    for (int i = 0; i < count; i++)
                        Read0xEKey0x13Key0x8(reader);
                    break;
                default:
                    Console.WriteLine($"Unhandled key '{key}' (Read0xEKey0x13)");
                    Console.ReadLine();
                    break;
            }
        }

        private void Read0xEKey0x13Key0x0(BinaryReader reader)
        {
            reader.ReadInt32();
        }

        private void Read0xEKey0x13Key0x8(BinaryReader reader)
        {
            reader.ReadInt32();  //53 (ItemId: 296108)
        }

        private void Read0xEKey0x13Key0x2(BinaryReader reader)
        {
            reader.ReadInt32();  //4 (ItemId: 205407)
        }

        private void Read0xEKey0x13Key0x5(BinaryReader reader)
        {
            reader.ReadInt32();  //43 (ItemId: 154939)
        }

        private void Read0xEKey0x13Key0x3(BinaryReader reader)
        {
            reader.ReadInt32();  //163 (ItemId: 117610)
        }

        private void Read0xEKey0x13Key0xB(BinaryReader reader)
        {
            reader.ReadInt32();  //1034 (ItemId: 100237)
        }

        private void ReadRequirements(BinaryReader reader)
        {
            var key = reader.ReadInt32();

            switch (key)
            {
                case 0x24:
                    var count = reader.Read3F1();
                    for (int i = 0; i < count; i++)
                        Read0x16Key0x24(reader);
                    break;
                default:
                    Console.WriteLine($"Unhandled key '{key}' (ReadRequirements)");
                    Console.ReadLine();
                    break;
            }
        }

        private void Read0x16Key0x24(BinaryReader reader)
        {
            ActionType key = (ActionType)reader.ReadInt32();
            var count = reader.Read3F1();

            switch (key)
            {
                case ActionType.ToUse:
                    for (int i = 0; i < count; i++)
                        Requirement.AddCriterion(key, new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));
                    break;
                case ActionType.ToWear:
                    for (int i = 0; i < count; i++)
                        Requirement.AddCriterion(key, new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));
                    break;
                case ActionType.ToWield:
                    for (int i = 0; i < count; i++)
                        Requirement.AddCriterion(key, new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));
                    break;
                case ActionType.UseItemOnItem:
                    for (int i = 0; i < count; i++)
                        Requirement.AddCriterion(key, new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));
                    break;
                case ActionType.PlayShiftRequirements:
                    for (int i = 0; i < count; i++)
                        Requirement.AddCriterion(key, new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));
                    break;
                case ActionType.Any:
                    for (int i = 0; i < count; i++)
                        Requirement.AddCriterion(key, new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));
                    break;
                case ActionType.ToTriggerTargetInVicinity:
                    for (int i = 0; i < count; i++)
                        Requirement.AddCriterion(key, new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));
                    break;
                case ActionType.Get:
                    for (int i = 0; i < count; i++)
                        Requirement.AddCriterion(key, new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));
                    break;
                case ActionType.ToRemove:
                    for (int i = 0; i < count; i++)
                        Requirement.AddCriterion(key, new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));
                    break;
                case ActionType.UseItemOnCharacter:
                    for (int i = 0; i < count; i++)
                        Requirement.AddCriterion(key, new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));
                    break;
                default:
                    Console.WriteLine($"Unhandled key '{key}' (Read0x16Key0x24)");
                    Console.ReadLine();
                    break;
            }
        }

        private void ReadUnk0x14(BinaryReader reader)
        {
            var key = reader.ReadInt32();

            switch (key)
            {
                case 0x5:
                    ReadUnk0x14Key0x5(reader);
                    break;
                default:
                    Console.WriteLine($"Unhandled key '{key}' (Read0x14)");
                    Console.ReadLine();
                    break;
            }
        }

        private void ReadUnk0x14Key0x5(BinaryReader reader)
        {
            var count = reader.Read3F1();

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadInt32();

                switch (key)
                {
                    case 0x0:
                    case 0x2:
                    case 0x3:
                    case 0x4:
                    case 0x5:
                    case 0xB:
                    case 0xF:
                    case 0x11:
                    case 0x12:
                    case 0x15:
                    case 0x16:
                    case 0x17:
                    case 0x1C:
                    case 0x1D:
                    case 0x1F:
                    case 0x31:
                    case 0x50:
                    case 0x64:
                    case 0x66:
                    case 0x73:
                    case 0x74:
                    case 0x83:
                    case 0x84:
                    case 0x87:
                    case 0x6F:
                        var count2 = reader.Read3F1();
                        for (int x = 0; x < count2; x++)
                            reader.ReadUInt32();
                        break;
                    default:
                        Console.WriteLine($"Unhandled key '{key} (Hex: {key.ToString("X2")}) (ReadUnk0x14Key0x5)");
                        Console.ReadLine();
                        break;
                }
            }
        }

        private void ReadSkills(BinaryReader reader)
        {
            var key = reader.ReadInt32();

            switch (key)
            {
                case 0x4:
                    AttackAndDefenseSkills(reader);
                    break;
                default:
                    Console.WriteLine($"Unhandled key '{key}' (ReadSkills)");
                    Console.ReadLine();
                    break;
            }
        }

        private void ReadModifiers(BinaryReader reader)
        {
            EventType key = (EventType)reader.ReadInt32();
            OnModifier(reader, key);
        }

        private void OnModifier(BinaryReader reader, EventType eventType)
        {
            var count = reader.Read3F1();

            for (int i = 0; i < count; i++)
            {
                FunctionType spellFunction = (FunctionType)reader.ReadInt32();
                var operations = new Dictionary<FunctionOperator, object>();

                reader.ReadUInt32();  // 0x0
                reader.ReadUInt32();  // 0x4
                var criterionCount = reader.ReadUInt32();  // 0x0
                List<RequirementCriterion> criterions = new List<RequirementCriterion>();

                for (int c = 0; c < criterionCount; c++)
                    criterions.Add(new RequirementCriterion(reader.ReadInt32(), reader.ReadUInt32(), (Operator)reader.ReadInt32()));

                if (criterions.Count != 0)
                    operations.Add(FunctionOperator.Criteria, criterions);

                operations.Add(FunctionOperator.Duration, reader.ReadInt32());
                operations.Add(FunctionOperator.Interval, reader.ReadUInt32());
                operations.Add(FunctionOperator.ApplyOn, reader.ReadUInt32());
                operations.Add(FunctionOperator.TargetList, reader.ReadUInt32());

                switch (spellFunction)
                {
                    case FunctionType.Hit:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Min, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Max, reader.ReadUInt32());
                        operations.Add(FunctionOperator.DamageType, reader.ReadUInt32());
                        break;
                    case FunctionType.GfxEffect: // Recheck order
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        operations.Add(FunctionOperator.GfxLife, reader.ReadUInt32()); 
                        operations.Add(FunctionOperator.GfxSize, reader.ReadUInt32()); 
                        operations.Add(FunctionOperator.GfxRed, reader.ReadUInt32());  
                        operations.Add(FunctionOperator.GfxGreen, reader.ReadUInt32());
                        operations.Add(FunctionOperator.GfxBlue, reader.ReadUInt32()); 
                        operations.Add(FunctionOperator.GfxFade, reader.ReadUInt32()); 
                        break;
                    case FunctionType.LockSkill:
                        operations.Add(FunctionOperator.Action, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.SystemText:
                        operations.Add(FunctionOperator.Text, Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32())));
                        operations.Add(FunctionOperator.Arg1, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Arg2, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Arg3, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Arg4, reader.ReadUInt32());
                        operations.Add(FunctionOperator.ToClient, reader.ReadUInt32());
                        break;
                    case FunctionType.UploadNano:
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.RestrictAction:
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimedLength, reader.ReadUInt32());
                        break;
                    case FunctionType.ModifyStat:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.Texture:
                        operations.Add(FunctionOperator.Texture, reader.ReadUInt32());
                        operations.Add(FunctionOperator.BodyPart, reader.ReadUInt32());
                        operations.Add(FunctionOperator.SeeSkin, reader.ReadUInt32());
                        break;
                    case FunctionType.ChangeBodyMesh:
                        operations.Add(FunctionOperator.BodyCatMesh, Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()))); //Not 100% sure if this is the correct Operator
                        break;
                    case FunctionType.EquipMonsterWeapon:
                        operations.Add(FunctionOperator.Hash2, Encoding.Default.GetString(reader.ReadBytes(4).Reverse().ToArray()));
                        break;
                    case FunctionType.AttractorEffect:
                        operations.Add(FunctionOperator.Unk86, reader.ReadUInt32());
                        operations.Add(FunctionOperator.ScreenVfx, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk89, reader.ReadUInt32());
                        break;
                    case FunctionType.AttractorEffect1:
                        operations.Add(FunctionOperator.Unk86, reader.ReadUInt32());
                        operations.Add(FunctionOperator.ScreenVfx, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk89, reader.ReadUInt32());
                        break;
                    case FunctionType.AttractorEffect2:
                        operations.Add(FunctionOperator.Unk86, reader.ReadUInt32());
                        operations.Add(FunctionOperator.ScreenVfx, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk89, reader.ReadUInt32());
                        break;
                    case FunctionType.HeadText:
                        operations.Add(FunctionOperator.Text, Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32())));
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.AddSkill:
                        operations.Add(FunctionOperator.Action, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.SetFlag:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.OpenBank:
                        break;
                    case FunctionType.MonsterShape:
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimedLength, reader.ReadUInt32());
                        break;
                    case FunctionType.CanFly:
                        break;
                    case FunctionType.ChangeVariable:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.Set:
                        operations.Add(FunctionOperator.Action, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.SpawnItem:
                        operations.Add(FunctionOperator.Hash, Encoding.Default.GetString(reader.ReadBytes(4).Reverse().ToArray()));
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimeExist, reader.ReadInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        operations.Add(FunctionOperator.RelXPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelYPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelZPos, reader.ReadInt32());
                        break;
                    case FunctionType.Teleport:
                        operations.Add(FunctionOperator.RelXPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelYPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelZPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.TeleportDest, reader.ReadUInt32());
                        break;
                    case FunctionType.SpawnMonster2:
                        operations.Add(FunctionOperator.Hash, Encoding.Default.GetString(reader.ReadBytes(4).Reverse().ToArray()));
                        operations.Add(FunctionOperator.SkillType, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimeExist, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        operations.Add(FunctionOperator.RelXPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelYPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelZPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.Unk2, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk1, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk3, reader.ReadUInt32());
                        break;
                    case FunctionType.SaveChar:  // Recheck order
                        operations.Add(FunctionOperator.RelXPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelYPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelZPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.TeleportDest, reader.ReadUInt32());
                        break;
                    case FunctionType.HeadMesh:  // Recheck order
                        operations.Add(FunctionOperator.Texture, reader.ReadUInt32());
                        operations.Add(FunctionOperator.MeshEffect, reader.ReadUInt32());
                        break;
                    case FunctionType.CreateApartment:  // Recheck order
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.EnterApartment:  // Recheck order
                        break;
                    case FunctionType.GenerateName:
                        var name = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));
                        var surname = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));
                        operations.Add(FunctionOperator.Text, new string[2] { name, surname });
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.Text:
                        var text1 = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));
                        var text2 = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));

                        operations.Add(FunctionOperator.Text, new string[2] { text1, text2 });
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.ResistNanoStrain:
                        operations.Add(FunctionOperator.NanoProperty, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.ClearFlag:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.CastChance:
                        operations.Add(FunctionOperator.ItemId, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.AreaHit:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Min, reader.ReadInt32());
                        operations.Add(FunctionOperator.Max, reader.ReadInt32());
                        operations.Add(FunctionOperator.DamageType, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Radius, reader.ReadUInt32());
                        break;
                    case FunctionType.BackMesh:
                        operations.Add(FunctionOperator.Texture, reader.ReadUInt32());
                        operations.Add(FunctionOperator.MeshEffect, reader.ReadUInt32());
                        break;
                    case FunctionType.SummonPet: //Flags, Unk2 might need swapping
                        operations.Add(FunctionOperator.Hash, Encoding.Default.GetString(reader.ReadBytes(4).Reverse().ToArray()));
                        operations.Add(FunctionOperator.HighItemIdMaxQl, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimeExist, reader.ReadInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        operations.Add(FunctionOperator.RelXPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelYPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelZPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.Unk2, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk1, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk3, reader.ReadUInt32());
                        operations.Add(FunctionOperator.PetReq1, reader.ReadUInt32());
                        operations.Add(FunctionOperator.PetReq2, reader.ReadUInt32());
                        operations.Add(FunctionOperator.PetReq3, reader.ReadUInt32());
                        operations.Add(FunctionOperator.PetReqVal1, reader.ReadUInt32());
                        operations.Add(FunctionOperator.PetReqVal2, reader.ReadUInt32());
                        operations.Add(FunctionOperator.PetReqVal3, reader.ReadUInt32());
                        break;
                    case FunctionType.TauntNpc: // Radius, Unk10, Unk11 might need swapping
                        operations.Add(FunctionOperator.Radius, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimedLength, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk10, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk11, reader.ReadUInt32());
                        break;
                    case FunctionType.NpcWipeHateList:
                        break;
                    case FunctionType.AttractorMesh:
                        operations.Add(FunctionOperator.BodyPart, reader.ReadUInt32());
                        operations.Add(FunctionOperator.MeshEffect, reader.ReadUInt32());
                        break;
                    case FunctionType.ScreenVfx:  // Flags TimedLength
                        operations.Add(FunctionOperator.ScreenVfx, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimedLength, reader.ReadUInt32());
                        break;
                    case FunctionType.CastNano:
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.ModifyNanoStat:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        operations.Add(FunctionOperator.SkillType, reader.ReadUInt32());
                        break;
                    case FunctionType.TeleportProxy2:
                        operations.Add(FunctionOperator.PfModelIdentityType, reader.ReadUInt32());
                        operations.Add(FunctionOperator.PfModelIdentityInstance, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk4, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk5, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk6, reader.ReadUInt32()); // 186A1 100001 ItemId: 154040
                        operations.Add(FunctionOperator.Unk7, reader.ReadUInt32()); // 1078D 67469  ItemId: 154040
                        operations.Add(FunctionOperator.Unk8, reader.ReadUInt32());
                        break;
                    case FunctionType.ModifyTemp:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        operations.Add(FunctionOperator.Radius, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Icon, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimedLength, reader.ReadUInt32());
                        break;
                    case FunctionType.SpawnQuest:
                        operations.Add(FunctionOperator.Hash, Encoding.Default.GetString(reader.ReadBytes(4).Reverse().ToArray()));
                        operations.Add(FunctionOperator.Unk9, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        break;
                    case FunctionType.AreaCastNano:
                        operations.Add(FunctionOperator.ItemId, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Radius, reader.ReadUInt32());
                        break;
                    case FunctionType.ShoulderMesh:
                        operations.Add(FunctionOperator.Texture, reader.ReadUInt32());
                        operations.Add(FunctionOperator.MeshEffect, reader.ReadUInt32());
                        break;
                    case FunctionType.DestroyItem:
                        break;
                    case FunctionType.EndFight:
                        operations.Add(FunctionOperator.Unk21, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        break;
                    case FunctionType.LandControlCreate:
                        break;
                    case FunctionType.ChangeActionRestriction:
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimedLength, reader.ReadUInt32());
                        break;
                    case FunctionType.ComboNameGen:
                        break;
                    case FunctionType.RemoveNanoEffect:
                        operations.Add(FunctionOperator.HighItemIdMaxQl, reader.ReadUInt32());
                        operations.Add(FunctionOperator.NanoSchool, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.NpcPushScript:
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.ModifyLevelScaling:
                        operations.Add(FunctionOperator.Stat, reader.ReadInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.DisableDefenseShield:
                        break;
                    case FunctionType.ReduceNanoStrainDuration:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.TeamCastNano:
                        operations.Add(FunctionOperator.ItemId, reader.ReadUInt32());
                        break;
                    case FunctionType.DrainHit:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Min, reader.ReadInt32());
                        operations.Add(FunctionOperator.Max, reader.ReadInt32());
                        operations.Add(FunctionOperator.DamageType, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.AddAction:
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Hash, Encoding.Default.GetString(reader.ReadBytes(4).Reverse().ToArray()));
                        operations.Add(FunctionOperator.HighItemIdMaxQl, reader.ReadInt32());
                        operations.Add(FunctionOperator.ItemId, reader.ReadUInt32());
                        break;
                    case FunctionType.LockPerk:
                        operations.Add(FunctionOperator.Action, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.HitPerk:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Min, reader.ReadInt32());
                        operations.Add(FunctionOperator.Max, reader.ReadInt32());
                        operations.Add(FunctionOperator.DamageType, reader.ReadUInt32());
                        break;
                    case FunctionType.Polymorph:
                        operations.Add(FunctionOperator.CatMeshEffect, reader.ReadUInt32());
                        operations.Add(FunctionOperator.AnimEffect, reader.ReadUInt32());
                        break;
                    case FunctionType.SaveHere:
                        break;
                    case FunctionType.TeleportProxy:
                        operations.Add(FunctionOperator.PfModelIdentityType, reader.ReadUInt32());
                        operations.Add(FunctionOperator.PfModelIdentityInstance, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk4, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk5, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk6, reader.ReadUInt32()); // 186A1 100001 ItemId: 225416
                        operations.Add(FunctionOperator.Unk7, reader.ReadUInt32()); // 111B2 70066  ItemId: 225416
                        break;
                    case FunctionType.RemoveNanoStrain:
                        operations.Add(FunctionOperator.Arg1, reader.ReadUInt32());
                        break;
                    case FunctionType.Anim: //Pos,Speed
                        operations.Add(FunctionOperator.Pos, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Play, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Speed, reader.ReadUInt32());
                        break;
                    case FunctionType.DeleteQuest:
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.NpcSocialAnim: //Pos,Speed
                        operations.Add(FunctionOperator.Unk23, reader.ReadUInt32());
                        break;
                    case FunctionType.CastOnPets:
                        operations.Add(FunctionOperator.ItemId, reader.ReadUInt32());
                        break;
                    case FunctionType.SetAnchor:
                        break;
                    case FunctionType.AttractorGfxEffect: //Unk12 Unk13 Unk15 Unk16 Unk17 Unk20 Unk86 and Unk14 Unk19
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        operations.Add(FunctionOperator.GfxLife, reader.ReadUInt32());
                        operations.Add(FunctionOperator.GfxSize, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk12, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk13, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk14, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk15, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk16, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk17, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk19, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk20, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk86, reader.ReadUInt32());
                        break;
                    case FunctionType.CastNanoIfPossible:
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.CastOnPf:
                        operations.Add(FunctionOperator.ItemId, reader.ReadUInt32());
                        break;
                    case FunctionType.SolveQuest:
                        operations.Add(FunctionOperator.Hash, Encoding.Default.GetString(reader.ReadBytes(4).Reverse().ToArray()));
                        break;
                    case FunctionType.DelayedSpawnNpc:
                        operations.Add(FunctionOperator.Hash, Encoding.Default.GetString(reader.ReadBytes(4).Reverse().ToArray()));
                        operations.Add(FunctionOperator.HighItemIdMaxQl, reader.ReadUInt32());
                        operations.Add(FunctionOperator.RelXPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelYPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelZPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.Unk151, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimeExist, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk168, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        break;
                    case FunctionType.SpawnMonsterRot: // Unk1 Unk2 Unk3 Flags
                        operations.Add(FunctionOperator.Hash, Encoding.Default.GetString(reader.ReadBytes(4).Reverse().ToArray()));
                        operations.Add(FunctionOperator.HighItemIdMaxQl, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimeExist, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk1, reader.ReadUInt32());
                        operations.Add(FunctionOperator.RelXPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelYPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelZPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.Unk2, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk3, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk151, reader.ReadUInt32());
                        break;
                    case FunctionType.RunScript:
                        operations.Add(FunctionOperator.RelXPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.RelZPos, reader.ReadInt32());
                        operations.Add(FunctionOperator.TimeExist, reader.ReadUInt32());
                        break;
                    case FunctionType.InstanceLock:
                        break;
                    case FunctionType.AddOffProc:
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        operations.Add(FunctionOperator.ItemInstance, reader.ReadInt32());
                        break;
                    case FunctionType.InputBox:
                        operations.Add(FunctionOperator.Text, Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32())));
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk117, reader.ReadUInt32());
                        break;
                    case FunctionType.KnockBack:
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Radius, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimeExist, reader.ReadUInt32());
                        break;
                    case FunctionType.AnimEffect:
                        operations.Add(FunctionOperator.TargetType, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TargetInstance, reader.ReadUInt32());
                        operations.Add(FunctionOperator.AnimEffect, reader.ReadUInt32());
                        operations.Add(FunctionOperator.AnimFlag, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Radius, reader.ReadUInt32());
                        break;
                    case FunctionType.RegisterControlPoint:
                        operations.Add(FunctionOperator.Unk101, reader.ReadUInt32());
                        break;
                    case FunctionType.AddBattleStationQueue:
                        operations.Add(FunctionOperator.Unk101, reader.ReadUInt32());
                        break;
                    case FunctionType.AddDefProc:
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        operations.Add(FunctionOperator.ItemInstance, reader.ReadInt32());
                        break;
                    case FunctionType.NpcSayRobotSpeech:
                        operations.Add(FunctionOperator.Text, Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32())));
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.RefreshModel:
                        break;
                    case FunctionType.CastBuff:
                        operations.Add(FunctionOperator.ItemId, reader.ReadUInt32());
                        break;
                    case FunctionType.ResetAllPerks:
                        break;
                    case FunctionType.InstancedPlayerCity:
                        operations.Add(FunctionOperator.PfModelIdentityType, reader.ReadUInt32());
                        operations.Add(FunctionOperator.PfModelIdentityInstance, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk4, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk5, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk6, reader.ReadUInt32());  // 186A1 100001 (ItemId: 281586)
                        operations.Add(FunctionOperator.Unk7, reader.ReadUInt32());  // 1177A 71546  (ItemId: 281586)
                        operations.Add(FunctionOperator.Unk8, reader.ReadUInt32());
                        break;
                    case FunctionType.CreateCityGuestKey:
                        operations.Add(FunctionOperator.Unk6, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk7, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Unk90, reader.ReadUInt32());
                        break;
                    case FunctionType.NpcMovementAction:
                        operations.Add(FunctionOperator.Unk150, reader.ReadUInt32());
                        break;
                    case FunctionType.Announcement: //RegenInterval TimeExist PfModelIdentityInstance
                        operations.Add(FunctionOperator.Text, Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32())));
                        operations.Add(FunctionOperator.PfModelIdentityInstance, reader.ReadUInt32());
                        operations.Add(FunctionOperator.RegenInterval, reader.ReadUInt32());
                        operations.Add(FunctionOperator.TimeExist, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Flags, reader.ReadUInt32());
                        break;
                    case FunctionType.Unk53248:
                        operations.Add(FunctionOperator.Type, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.ChangeBreed:
                        operations.Add(FunctionOperator.Breed, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Sex, reader.ReadInt32());
                        break;
                    case FunctionType.ChangeGender:
                        operations.Add(FunctionOperator.Sex, reader.ReadInt32());
                        break;
                    case FunctionType.GoToLastSavePoint:
                        break;
                    case FunctionType.Faction:
                        operations.Add(FunctionOperator.Stat, reader.ReadUInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.SendMail: // Unk20 HighItemIdMax
                        var sendTo = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));
                        var title = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));
                        var desc = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));
                        operations.Add(FunctionOperator.Text, new string[3] { sendTo, title, desc });
                        operations.Add(FunctionOperator.Hash, Encoding.Default.GetString(reader.ReadBytes(4).Reverse().ToArray()));
                        operations.Add(FunctionOperator.Unk20, reader.ReadUInt32());
                        operations.Add(FunctionOperator.HighItemIdMaxQl, reader.ReadInt32());
                        operations.Add(FunctionOperator.Arg1, reader.ReadInt32());
                        break;
                    case FunctionType.FailQuest:
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.Transfer:
                        operations.Add(FunctionOperator.Flags, reader.ReadInt32());
                        operations.Add(FunctionOperator.Value, reader.ReadUInt32());
                        break;
                    case FunctionType.NextHead:
                        break;
                    case FunctionType.PrevHead:
                        break;
                    case FunctionType.SetGovernmentType:
                        operations.Add(FunctionOperator.Value, reader.ReadInt32());
                        break;
                    case FunctionType.Charge:
                        operations.Add(FunctionOperator.Speed, reader.ReadInt32());
                        break;
                    default:
                        Console.WriteLine($"Unhandled key '{spellFunction}: {eventType}' (OnModifier)");
                        Console.ReadLine();
                        return;
                }

                switch (eventType)
                {
                    case EventType.OnWear:
                        OnWear.AddModifier(spellFunction, operations);
                        break;
                    case EventType.OnUse:
                        OnUse.AddModifier(spellFunction, operations);
                        break;
                    case EventType.OnActivate:
                        OnActivate.AddModifier(spellFunction, operations);
                        break;
                    case EventType.OnEnter:
                        OnEnter.AddModifier(spellFunction, operations);
                        break;

                }
            }
        }

        private void ReadUnk0x6(BinaryReader reader)
        {
            int key = reader.ReadInt32();

            switch (key)
            {
                case 0x1b:
                    int count = reader.Read3F1();

                    for (int x = 0; x < count; x++)
                    {
                        var unk1 = reader.ReadInt32();
                        var unk2 = reader.ReadUInt32();

                        if (!UnkSkills01.ContainsKey(unk1)) //156928 has x2 of the same unk skill
                            UnkSkills01.Add(unk1, unk2);
                    }
                    break;
                default:
                    Console.WriteLine($"Unhandled key '{key}' (UnkSkills1)");
                    Console.ReadLine();
                    return;
            }
        }

        private void AttackAndDefenseSkills(BinaryReader reader)
        {
            int totalSkillCount = reader.Read3F1();

            for (int x = 0; x < totalSkillCount; x++)
            {
                var skillType = reader.ReadInt32();
                int skillCount = reader.Read3F1();

                switch (skillType)
                {
                    case 0x0D://Defense Skills
                        for (int y = 0; y < skillCount; y++)
                        {
                            var stat = (StatId)reader.ReadInt32();
                            var value = reader.ReadUInt32();

                            if (!DefenseSkills.ContainsKey(stat)) //156928 has x2 of the same def skill
                                DefenseSkills.Add(stat,value);
                        }

                        break;
                    case 0x0C: //Attack Skills
                        for (int y = 0; y < skillCount; y++)
                        {
                            var stat = (StatId)reader.ReadInt32();
                            var value = reader.ReadUInt32();

                            if (!AttackSkills.ContainsKey(stat))//156928 has x2 of the same attack skill
                                AttackSkills.Add(stat, value);
                        }
                        break;
                    case 0x03: //297805
                        for (int y = 0; y < skillCount; y++)
                        {
                            var stat = (StatId)reader.ReadInt32();
                            var value = reader.ReadUInt32();

                            //if (!AttackSkills.ContainsKey(stat))//156928 has x2 of the same attack skill
                                //AttackSkills.Add(stat, value);
                        }
                        break;
                    default:
                        Console.WriteLine($"Unhandled key '{skillType}' (AttackAndDefenseSkills)");
                        Console.ReadLine();
                        return;
                }
            }
        }

        private void ReadText(BinaryReader reader)
        {
            TextIdentifier key = (TextIdentifier)reader.ReadInt32();

            switch (key)
            {
                case TextIdentifier.NameAndDesc:
                    ReadNameAndDesc(reader);
                    break;
                default:
                    Console.WriteLine($"Unhandled key '{key}' (ReadText)");
                    Console.ReadLine();
                    return;
            }
        }

        public void ReadStats(BinaryReader reader)
        {
            int potentialKey = reader.ReadInt32();

            switch (potentialKey)
            {
                case 0x25: // Not sure what these mean
                    var count = reader.Read3F1();
                    for (int i = 0; i < count; i++)
                    {
                        reader.ReadUInt32();
                        var key = reader.ReadInt16();
                        reader.ReadInt16();
                        if (key == 0)
                        {
                            reader.ReadBytes(13);
                        }
                        else
                        {
                            reader.ReadBytes(19);
                        }
                    }
                    break;
                default:
                    var count2 = ((float)potentialKey / 1009) - 1;

                    if ((int)count2 != count2) // We will use this until we map out all identifiers
                    {
                        Console.WriteLine($"Unhandled key '{potentialKey}' (ReadStats)");
                        Console.ReadLine();
                    }
                    else
                    {
                        for (int i = 0; i < count2; i++)
                            Stats.Add((StatId)reader.ReadInt32(), reader.ReadUInt32());
                    }
                    return;
            }
        }

        public void ReadNameAndDesc(BinaryReader reader)
        {
            int nameLength = reader.ReadUInt16();
            int descLength = reader.ReadUInt16();

            Name = Encoding.Default.GetString(reader.ReadBytes(nameLength));
            Description = Encoding.Default.GetString(reader.ReadBytes(descLength));
        //    Console.WriteLine($"Name: {name}");
        //    Console.WriteLine($"Desc: {desc}");
        }


        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}

public enum ItemKeyIdentifier
{
    Text = 0x15,
    Stats = 0x17,
    Skills = 0x4,
    Modifiers = 0x2,
    Requirements = 0x16,
}

public enum TextIdentifier
{
    NameAndDesc = 0x21
}