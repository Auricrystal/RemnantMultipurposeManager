   M S B A M L   `   `   `  ���� R  NRemnantBuildRandomizer, Version=2.12.4.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:RemnantBuildRandomizerRemnantBuildRandomizer  '    !RemnantBuildRandomizer.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     6local$clr-namespace:RemnantBuildRandomizer     -   $��RemnantBuildRandomizer��  � Title$  Remnant Build Randomizer��$��711.531��$��837.522�� � Icon$' !Resources/IMG/Menu/RedCrystal.png�����#222222$��711.531��$��837.522�� � 	Resources    windowScaleTransform �� ScaleX �� ScaleY�� %#   &	        �� $	 0.5Q�$	 0.5Q�.��� -   $��MainGrid�����#222222.+�o� $��23��$	��Top=�.@�m� $��Options��.@�m� -   $��Copy Build to Clipboard��m� -   $��Check for Update��m� -   $��Power Cycle Game��m� $
��View��.@�m� -   $��Data Folder���� -   $��cmbCharacter��$	��120���� -   $��cmbSaveSlot��$	��120���� -	   $��SaveTime��$��
Last Save:���� $��0,23,0,0q����#222222.@��� $��Build Generator��.��(�  (� Stretch$ Uniform=� (� StretchDirection$
 Both=�.��� ���#222222$��-2,-2,-2,-2q�$��Center=�$��Center=�.+�� -
   $��BuildScreen��$��	0,0,340,0q�$��Center=�$��Center=�.+��� -   $��	BuildView��$-��'Resources/IMG/Menu/RemnantBuildView.png��$	��489��$	��622���� -   $��
HandGunImg��$9��3Resources/IMG/Weapons/Hand/RegHand/_No Hand Gun.png��$��
5,10,5,525q�$��Uniform=�$	��396��$��99���� -   $��HandGunText��$��Hand Gun��$��10,16,173,602q�
�������$��15(�$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)��� -   $��
HandModImg��$0��*Resources/IMG/Mods/RegularMods/_No Mod.png��$��374,14,5,529q�$��91��$��91���� -   $��HandGunModText��$��Hand Mod�� �� TextAlignment$ Right=�$��234,95,104,534q�
�������$��15(�$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)��� -   $��HandCoverModImg��$(��"Resources/IMG/Mods/Mod Overlay.png��$��374,14,5,529q�$��91��$��91���� -   $��
LongGunImg��$9��3Resources/IMG/Weapons/Long/RegLong/_No Long Gun.png��$��5,118,5,416q�$��Uniform=�$	��400��$	��100���� -   $��
LongModImg��$0��*Resources/IMG/Mods/RegularMods/_No Mod.png��$��374,123,5,420q�$��91��$��91���� -   $��LongCoverModImg��$(��"Resources/IMG/Mods/Mod Overlay.png��$��374,123,5,420q�$��91��$��91���� -   $��MeleeImg��$,��&Resources/IMG/Weapons/Melee/_Fists.png��$��44,227,45,319q�$��Uniform=�$	��400��$��90���� -   $��HeadImg��$+��%Resources/IMG/Armor/Head/_No Head.png��$��14,354,349,163q�$	��136��$	��136���� -    ��Name$ ChestImg��$-��'Resources/IMG/Armor/Chest/_No Chest.png��$��180,349,183,168q�$	��136��$	��136���� -   $ LegImg��$+��%Resources/IMG/Armor/Legs/_No Legs.png��$��344,349,19,168q�$	��136��$	��136���� -   $ AmImg��$3��-Resources/IMG/Trinkets/Amulets/_No Amulet.png��$��20,507,343,10q�$	��136��$	��136���� -   $ Ri1Img��$/��)Resources/IMG/Trinkets/Rings/_No Ring.png��$��180,507,183,10q�$	��136��$	��136���� -   $ Ri2Img��$/��)Resources/IMG/Trinkets/Rings/_No Ring.png��$��344,507,19,10q�$	��136��$	��136���� -   	 ��Name$	 LongGunText��$��Long Gun��$��10,133,339,487q�
�������$��15(�$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)�$	��150��$��33���� -   $	 LongGunModText��$��Long Mod��$ Right=�$��245,202,104,425q�
�������$��15(�$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)�$	��150���� -   $	 	MeleeText��$��Melee��$��10,236,339,384q�
�������$��15(�$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)�$	��150��$��33���� -   $	 HeadText��$
��Head��$��14,343,335,277q�
�������$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)�$	��150��$��33���� -   $	 	ChestText��$��Chest��$��176,343,173,277q�
�������$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)�$	��150��$��33���� -    $	 LegText��$
��Legs��$��339,343,10,277q�
�������$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)�$	��150��$��33���� -!   $	 
AmuletText��$��Amulet��$��WrapWithOverflow=�$��10,488,331,117q�
�������$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)�$	��150��$��33���� -"   $	 	Ring1Text��$��Ring 1��$��176,495,173,125q�
�������$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)�$	��150��$��33���� -#   $	 	Ring2Text��$��Ring 2��$��339,495,10,125q�
�������$C��=/RemnantBuildRandomizer;component/Resources/Fonts/#Montserrat)�$	��150��$��33��� $��	490,6,0,0q�$
��Left=�$	��Top=�$	��620��.+��� -$   $��SideView��$&�� Resources/IMG/Menu/SidePanel.png��$��UniformToFill=�$��0,38,0,0q�$	��343���� -%   $��BuildCodeView��$%��Resources/IMG/Menu/TopPanel.png��$
��Fill=�$��	0,0,0,587q�$	��343��$��47���� -&   $��RerollBuild��$��Reroll Build��$
��Left=�$��62��$��
53,539,0,0q�$	��Top=�$	��224��$��24(��� -'   $��BuildNum��$
��Left=�$��33��$
��Wrap=�$&�� 00-00-00-00-00-00-00-00-00-00-00��$	��Top=�$	��329��
�������$��20(�$��10,5,0,0q�
 � ContextMenu
 �� .@�m� -(   $��	BuildCode��$��Copy Build Code���� $
��Left=�$	��379��$��
53,123,0,0q�$	��Top=�$	��204��$.��(Resources/IMG/Menu/T_Icon_RedCrystal.png���� $��	Blacklist��.��(� $ Uniform=�$
 Both=�.��� ���#222222$
��Left=�$	��Top=�$	��681��$	��893��.+��� $��10,258,10,10q�$	��873��$	��413����Z� .@��� $��Weapons��.��&   System.Windows.Controls.DataGrid  -)   $��
WeaponList���� $��Armor��.��  -*   $��	ArmorList���� $��Amulets��.��  -+   $��
AmuletList���� $��Rings��.��  -,   $��RingList���� $��General Mods��.��  --   $��ModList���� -.   $��Reset Blacklist��$
��Left=�$��32��$��
10,139,0,0q�$	��Top=�$	��136���� $
��Left=�$	��117��$��10,3,0,0q�$	��Top=�$	��143��$6��0Resources/IMG/Menu/T_Icon_Item_WorldFragment.png���� $
��Left=�$	��225��$��	158,3,0,0q�$	��Top=�$	��218��$?��9Resources/IMG/Menu/T_Icon_Item_Resource_LaystoneShard.png���� $
��Left=�$	��234��$��	577,0,0,0q�$	��Top=�$	��237��$3��-Resources/IMG/Menu/T_Icon_Item_Simulacrum.png���� 
�������$��28(�$ Center=�$
��Left=�$��36��$��	352,1,0,0q�$
��Wrap=�$��Disable Empties��$	��Top=�$	��238��  -/   $��
EmptySlots��$
��Left=�$	��197��$��
352,37,0,0q�$	��Top=�$	��238���� -0   $��Disable All Empties��$
��Left=�$��32��$��
10,176,0,0q�$	��Top=�$	��136���� $��
Build List��.��(� $ Uniform=�$
 Both=�.��� ���#222222$
��Left=�$	��Top=�$��-2q�$	��620��$	��808�� � ColumnDefinitions �� .+��� $��-117,0,-156,0q�$!��Resources/IMG/Menu/Rigs.jpg��$��Center=�$
��1081��  -1   $��	BuildList��   SelectionMode$ Single=�$��352,199,10,10q����#222222
 �� .@�m� -2   $��GrabBuildImage��$��Grab Build Image��m� -3   $��CopyBuildObject��$��
Copy Build��m� -4   $��PasteBuildObject��$��Paste Build���� -5   $��BuildNameEnter��$
��Left=�$��22��$��352,150,0,0q� �� TextWrapping$
 Wrap=�$��
Build Name��$	��Top=�$	��173���� -6   $��BuildCodeEnter��$
��Left=�$��22��$��352,172,0,0q�$
 Wrap=�$��
Build Code��$	��Top=�$	��173���� -7   $��AddBuild��$��	Add Build��$
��Left=�$��22��$��530,172,0,0q�$	��Top=�$��84���� -8   $��cmbHG��$��Center=�$
��Left=�$��20��$��	13,18,0,0q�$	��Top=�$	��142���� -9   $��cmbLG��$��Center=�$
��Left=�$��20��$��	13,38,0,0q�$	��Top=�$	��142���� -:   $
��cmbM��$��Center=�$
��Left=�$��20��$��	13,58,0,0q�$	��Top=�$	��142���� -;   $��cmbHGM��$��Center=�$
��Left=�$��20��$��
160,18,0,0q�$	��Top=�$	��142���� -<   $��cmbLGM��$��Center=�$
��Left=�$��20��$��
160,38,0,0q�$	��Top=�$	��142���� -=   $��cmbHE��$��Center=�$
��Left=�$��20��$��	13,78,0,0q�$	��Top=�$	��142���� ->   $��cmbCH��$��Center=�$
��Left=�$��20��$��
160,78,0,0q�$	��Top=�$	��142���� -?   $��cmbLE��$��Center=�$
��Left=�$��20��$��
307,78,0,0q�$	��Top=�$	��142���� -@   $��cmbAM��$��Center=�$
��Left=�$��20��$��	13,98,0,0q�$	��Top=�$	��142���� -A   $��cmbR1��$��Center=�$
��Left=�$��20��$��
160,98,0,0q�$	��Top=�$	��142���� -B   $��cmbR2��$��Center=�$
��Left=�$��20��$��
307,98,0,0q�$	��Top=�$	��142���� -C    ��Name$ GenBuildCode��$��Generate Code��$
��Left=�$��22��$��196,134,0,0q�$	��Top=�$	��106���� $��37��$��0,1,10,0q�$
��Wrap=�$��Build Code Maker��$	��Top=�
�������$��27(�$��Right=�$	��253���� $��94��$��654,43,60,0q�$	��Top=�$,��&Resources/IMG/Menu/T_Icon_Crafting.png�� J� RenderTransformOrigin$ -0.355,1.261&��� $��Save Manipulator��.��� $	��824��$	��630��.+��� $
��Left=�$ Center=�$��61��$��0,10,0,0q�$
��Wrap=�$��Checkpoint Manager��$	��Top=�$	��374��$��36(�
��������� -D    ��Name$ 	AlterFile�� d� 	IsChecked$ FalseY�$��Alter Save File��$��Center=�$
��Left=�$��34��$��	525,0,0,0q�$	��Top=�$	��169��
�������$��22(��� $
��Wrap=�$����In order to preserve your personal save you must have this check box enabled, it is responsible for reverting back to your original save when you are finished.

DO NOT disable unless you are at the Main Menu.��
�������$��389,35,10,524q��� -E   $��KeepCheckpoint��$��Keep Current Checkpoint��$��20��$��150,71,462,539q�$��Center=�
�������$��16(��� $��	0,107,0,0q���Z� .@��� -F   $��BossManagerTab��$��Checkpoint Manager��.��(� $��Center=�$��Center=�$ UniformToFill=�$
 Both=�.��� $	��824��$	��496��.+��� $2��,Resources/IMG/Menu/remnant_of_mankind_bg.jpg��$��UniformToFill=�$��	0,-25,0,0q�$ 0.5,0.5&�z�V� .~��� ��  �� AngleX$
 0.11Q��� T�  T� X$ 0.499Q��� -G   $��LoadBoss��$��Load Checkpoint��$
��Left=�$��28��$��
422,35,0,0q�$	��Top=�$	��113��{���  �� ElementName$ 	AlterFile�� �� Path$ 	IsChecked��� -H   $��FeelingLucky��$��I'm Feeling Lucky�� � ToolTip$ Pick a random boss��$
��Left=�$��28��$��422,173,0,0q�$	��Top=�$	��113��{��� *	IsChecked+$ 	AlterFile���� $
��Wrap=�$r��lONLY reroll or choose Boss checkpoints while at the Main Menu, doing it during gameplay will have no effect.��
�������$��422,83,211,323q��� $��
0,10,412,0q�$	��452��$	��Top=�{��� *	IsChecked+$ 	AlterFile��.@��� $��
Boss Saves��.��  -I   $��BossList��$ Single=��� $��Vendor Saves��.��  -J   $��VendList��$ Single=��� $��Miscellaneous Saves��.��  -K   $��MiscList��$ Single=�
 �� .@�m� -L   $��AddFiles��$��	Add Files���� -M   $��CheckSearchBar��$
��Left=�$��Center=�$��25��$��	0,467,0,0q�$
 Wrap=�$�� ��$	��Top=�$	��412���� $��Page2���� $	��Log��.��� ���	#FFE5E5E5$��-2,-2,-2,-2q�.+��� -N    ��Name$ txtLog��$
 Wrap=�
�������" �� VerticalScrollBarVisibility$
 Auto=��� $��Credits��.��(� $ Uniform=�$
 Both=�.��� $	��824��$	��630��.+��� $��CoolUsername��$��24(�
�������$	��172��$��32��$��35,109,617,489q��� $��206,109,519,489q�$��32���� -O   $/��)https://www.youtube.com/user/CharlieM780/?�$��24(�
�������.�	Youtube�� $��Center=�
�������$��24(�$��
Boss Saves��$��333,109,319,489q�$	��172��$��32���� $��Bolt Jamison��$��24(�
�������$��34,72,618,526q�$	��172��$��32���� $��206,72,519,526q�$��32���� -P   $>��8https://www.youtube.com/channel/UCbkN9vpjqx-PDKSISbFPidQ?�$��24(�
�������.�	Youtube�� $��Center=�
�������$��24(�$��Boss Saves/Bug Fixes��$��334,72,262,526q�$��32���� $��Glaras��$��24(�
�������$��34,146,618,452q�$	��172��$��32���� $��206,146,519,452q�$��32���� -Q   $>��8https://www.youtube.com/channel/UCvq5gYVtRsqgiMkZ18Ixi2Q?�$��24(�
�������.�	Youtube�� $��Center=�
�������$��24(�$��Boss Saves/Bug Fixes��$��334,152,262,446q�$��32���� $��Anh Lam��$��24(�
�������$��34,40,618,558q�$	��172��$��32���� $��Center=�
�������$��18(�$'��!Smaller IMG sizes/Background IMGs��$��334,46,200,552q�$��32���� $��
Razzmatazz��$��24(�
�������$��34,183,618,415q�$	��172��$��32���� $��Center=�
�������$��24(�$��Save Reader Code��$��334,189,262,409q�$��32���� $��Additional Links��$��246,305,337,293q�$��30(�
�������$��32��$��	Underline{��� $��	devious21��$��24(�
�������$��34,342,618,256q�$	��172��$��32���� $��32��$��454,342,64,256q��� -R   $��https://wtgclan.com/?�$��24(�
�������.�Remnant Loadout Generator�� $��
Razzmatazz��$��24(�
�������$��34,416,618,182q�$	��172��$��32���� $��32��$��454,416,82,182q��� -S   $G��Ahttps://github.com/Razzmatazzz/RemnantSaveManager/releases/latest?�$��24(�
�������.�Razzmatazz Save Analyzer�� $��Gregolas��$��24(�
�������$��35,379,617,219q�$	��172��$��32���� $��32��$��454,379,64,219q��� -T   $)��#https://remnant-builds.glideapp.io/?�$��24(�
�������.�Remnant Builds Gallery�� $��32��$��206,379,519,219q��� -U   $1��+https://www.youtube.com/user/PianoSamurai88?�$��24(�
�������.�	Youtube