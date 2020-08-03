using DDONamedGearPlanner;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

sealed class TypeMapBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        Type typeToDeserialize;
        String exeAssembly = Assembly.GetExecutingAssembly().FullName;
        if(typeName.Contains("System.Collections.Generic"))
        {
            typeName = typeName.Replace("DDONamedGearPlanner, Version=1.0.0.0", "extract, Version=0.0.0.0");
            typeToDeserialize = Type.GetType(typeName);
        }
        else
        {
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, exeAssembly));
        }
        return typeToDeserialize;
    }
}

public class HelloWorld
{
    public static DDODataset Dataset;

    private static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
    {
        return typeof(DDODataset).Assembly;
    }

    private static void LoadDataset(){
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyResolve += new ResolveEventHandler(MyResolveEventHandler);

        FileStream fs = new FileStream("ddodata.dat", FileMode.Open);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Binder = new TypeMapBinder();
        Dataset = (DDODataset)bf.Deserialize(fs);
        fs.Close();
    }

    private static void WriteSlots(){
        Console.WriteLine(@"CREATE TABLE slot(slot_id integer primary key,
                                              name text unique not null,
                                              type text);");
        foreach(KeyValuePair<SlotType, DDOSlot> entry in Dataset.Slots){
            if(entry.Value.CategoryEnumType != null)
            {
                Console.WriteLine(String.Format("INSERT INTO slot(name,type) VALUES(\"{0}\",\"{1}\");",
                    entry.Value.Slot,
                    entry.Value.CategoryEnumType.ToString().Split(".").Last()));
            }
            else
            {
                Console.WriteLine(String.Format("INSERT INTO slot(name) VALUES(\"{0}\");", entry.Value.Slot));
            }
        }
    }

    private static void WriteProperties(){
        Console.WriteLine(@"CREATE TABLE property(property_id integer primary key,
                                                  name text unique not null);");
        Console.WriteLine(@"CREATE TABLE type(type_id integer primary key,
                                              name text unique not null);");
        Console.WriteLine(@"CREATE TABLE property_type(property_id integer not null,
                                                       type_id integer not null,
                                                       foreign key(property_id) references property(property_id) on delete cascade on update cascade,
                                                       foreign key(type_id) references type(type_id) on delete cascade on update cascade,
                                                       UNIQUE(property_id, type_id));");
        foreach(KeyValuePair<string, DDOItemProperty> entry in Dataset.ItemProperties){
            Console.WriteLine(String.Format("INSERT INTO property(name) VALUES(\"{0}\");", entry.Value.Property));
            foreach(String type in entry.Value.Types)
            {
                if(type.Length > 0)
                {
                    Console.WriteLine(String.Format("INSERT OR IGNORE INTO type(name) VALUES(\"{0}\");", type));
                    Console.WriteLine(String.Format("INSERT INTO property_type VALUES ((SELECT property_id FROM property WHERE name=\"{0}\"),(SELECT type_id FROM type WHERE name=\"{1}\"));",
                        entry.Value.Property,
                        type));
                }
            }
        }
    }

    private static void WriteQuests(){
        Console.WriteLine(@"CREATE TABLE pack(pack_id integer primary key,
                                              name text unique not null,
                                              FreeToVIP boolean not null);");
        Console.WriteLine(@"CREATE TABLE quest(quest_id integer primary key,
                                               name text unique not null,
                                               pack_id integer not null,
                                               isRaid boolean not null,
                                               isFree boolean not null,
                                               foreign key(pack_id) references pack(pack_id) on delete cascade on update cascade);");
        foreach(DDOAdventurePackData pack in Dataset.AdventurePacks)
        {
            Console.WriteLine(String.Format("INSERT INTO pack(name,FreeToVIP) VALUES(\"{0}\",{1});",
                pack.Name,
                Convert.ToInt32(pack.FreeToVIP)));
            foreach(DDOQuestData quest in pack.Quests)
            {
                if(quest.Name != null)
                {
                    Console.WriteLine(String.Format("INSERT OR IGNORE INTO quest(name,pack_id,isRaid,isFree) VALUES(\"{0}\",(SELECT pack_id FROM pack WHERE name=\"{1}\"),{2},{3});",
                        quest.Name,
                        pack.Name,
                        Convert.ToInt32(quest.IsRaid),
                        Convert.ToInt32(quest.IsFree)));
                }
            }
        }
    }

    private static void WriteItems(){
        Console.WriteLine(@"CREATE TABLE item(item_id integer primary key,
                                              name text unique not null,
                                              url text,
                                              armor_category text,
                                              offhand_category text,
                                              weapon_category text,
                                              ML integer not null,
                                              minor_artifact boolean not null,
                                              weapon_type text,
                                              handedness integer not null,
                                              source text not null,
                                              quest_id integer,
                                              foreign key(quest_id) references quest(quest_id) on delete cascade on update cascade);");
        Console.WriteLine(@"CREATE TABLE item_property(item_id integer not null,
                                                       property_id integer not null,
                                                       type text not null,
                                                       value float not null,
                                                       foreign key(item_id) references item(item_id) on delete cascade on update cascade,
                                                       foreign key(property_id) references property(property_id) on delete cascade on update cascade,
                                                       UNIQUE(item_id,property_id,type,value));");
        Console.WriteLine(@"CREATE TABLE item_slot(item_id integer not null,
                                                   slot_id integer not null,
                                                   foreign key(item_id) references item(item_id) on delete cascade on update cascade,
                                                   foreign key(slot_id) references slot(slot_id) on delete cascade on update cascade,
                                                   UNIQUE(item_id,slot_id));");
        foreach(DDOItemData item in Dataset.Items)
        {
            Console.WriteLine(String.Format("INSERT OR IGNORE INTO item(name,url,ML,minor_artifact,weapon_type,handedness,source,quest_id) VALUES(\"{0}\",\"{1}\",{2},{3},{4},{5},\"{6}\",(SELECT quest_id FROM quest WHERE name=\"{7}\"));",
                item.Name,
                item.WikiURL,
                item.ML,
                Convert.ToInt32(item.MinorArtifact),
                item.WeaponType == null ? "" : String.Format("\"{0}\"", item.WeaponType),
                item.Handedness,
                item.Source,
                item.QuestFoundIn.Name));
                Console.WriteLine(String.Format("INSERT INTO item_slot VALUES ((SELECT item_id FROM item WHERE name=\"{0}\"),(SELECT slot_id FROM slot WHERE name=\"{1}\"));",
                    item.Name,
                    item.Slot));
            foreach(ItemProperty prop in item.Properties){
                switch(prop.Property)
                {
                    case "Minimum Level":
                    case "Handedness":
                    case "Weapon Type":
                        break;
                    case "Armor Category":
                        Console.WriteLine("UPDATE item SET armor_category=\"{0}\" WHERE name=\"{1}\";",
                            (ArmorCategory)item.Category,
                            item.Name);
                        break;
                    case "Offhand Category":
                        Console.WriteLine("UPDATE item SET offhand_category=\"{0}\" WHERE name=\"{1}\";",
                            (OffhandCategory)item.Category,
                            item.Name);
                        break;
                    case "Weapon Category":
                        Console.WriteLine("UPDATE item SET weapon_category=\"{0}\" WHERE name=\"{1}\";",
                            (WeaponCategory)item.Category,
                            item.Name);
                        break;
                    default:
                        if(Dataset.Sets.ContainsKey(prop.Property))
                        {
                            break;
                        }
                        if(prop.Options == null)
                        {
                            Console.WriteLine(String.Format("INSERT OR IGNORE INTO item_property VALUES ((SELECT item_id FROM item WHERE name=\"{0}\"),(SELECT property_id FROM property WHERE name=\"{1}\"),{2},{3});",
                                item.Name,
                                prop.Property,
                                prop.Type == null ? "\"\"" : String.Format("\"{0}\"", prop.Type),
                                prop.Value));
                        }
                        else
                        {
                            foreach(ItemProperty prop2 in prop.Options)
                            {
                                Console.WriteLine(String.Format("INSERT OR IGNORE INTO item_property VALUES ((SELECT item_id FROM item WHERE name=\"{0}\"),(SELECT property_id FROM property WHERE name=\"{1}\"),{2},{3});",
                                    item.Name,
                                    prop2.Property,
                                    prop2.Type == null ? "\"\"" : String.Format("\"{0}\"", prop2.Type),
                                    prop2.Value));
                            }
                        }
                        break;
                }
            }
        }
    }

    private static void WriteSets(){
        Console.WriteLine(@"CREATE TABLE itemset(itemset_id integer primary key,
                                                 name text unique not null,
                                                 url text);");
        Console.WriteLine(@"CREATE TABLE itemset_item(itemset_id integer not null,
                                                      item_id integer not null,
                                                      foreign key(itemset_id) references itemset(itemset_id) on delete cascade on update cascade,
                                                      foreign key(item_id) references item(item_id) on delete cascade on update cascade,
                                                      UNIQUE(itemset_id,item_id));");
        Console.WriteLine(@"CREATE TABLE itemset_property(itemset_id integer not null,
                                                          property_id integer not null,
                                                          type text not null,
                                                          value float not null,
                                                          min_items integer not null,
                                                          foreign key(itemset_id) references itemset(itemset_id) on delete cascade on update cascade,
                                                          foreign key(property_id) references property(property_id) on delete cascade on update cascade,
                                                          UNIQUE(itemset_id,property_id,type,value));");
        foreach(KeyValuePair<string, DDOItemSet> entry in Dataset.Sets)
        {
            Console.WriteLine(String.Format("INSERT INTO itemset(name,url) VALUES(\"{0}\",{1});",
                entry.Value.Name,
                entry.Value.WikiURL == null ? "NULL" : String.Format("\"{0}\"", entry.Value.WikiURL)));
            foreach(DDOItemData item in entry.Value.Items)
            {
                Console.WriteLine(String.Format("INSERT OR IGNORE INTO itemset_item VALUES ((SELECT itemset_id FROM itemset WHERE name=\"{0}\"),(SELECT item_id FROM item WHERE name=\"{1}\"));",
                    entry.Value.Name,
                    item.Name));
            }
            if(entry.Value.SetBonuses != null)
            {
                foreach(DDOItemSetBonus bonus in entry.Value.SetBonuses.OrderBy(o=>o.MinimumItems))
                {
                    foreach(DDOItemSetBonusProperty prop in bonus.Bonuses)
                    {
                        Console.WriteLine(String.Format("INSERT OR IGNORE INTO itemset_property VALUES ((SELECT itemset_id FROM itemset WHERE name=\"{0}\"),(SELECT property_id FROM property WHERE name=\"{1}\"),{2},{3},{4});",
                            entry.Value.Name,
                            prop.Property,
                            prop.Type == null ? "\"\"" : String.Format("\"{0}\"", prop.Type),
                            prop.Value,
                            bonus.MinimumItems));
                    }
                }
            }
        }
    }

    public static void Main(string[] args)
    {
        LoadDataset();
        Console.Error.WriteLine("Dataset loaded successfully");

        Console.WriteLine("BEGIN TRANSACTION;");
        WriteSlots();
        WriteProperties();
        WriteQuests();
        WriteItems();
        WriteSets();
        Console.WriteLine("COMMIT;");

        Console.Error.WriteLine("Dump to ddodata.dump complete");
    }
}