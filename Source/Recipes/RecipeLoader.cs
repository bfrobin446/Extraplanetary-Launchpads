/*
This file is part of Extraplanetary Launchpads.

Extraplanetary Launchpads is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Extraplanetary Launchpads is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Extraplanetary Launchpads.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace ExLP {
	public class ExRecipeLoader: LoadingSystem
	{
		public bool done;

		void LoadDefautStructureRecipe ()
		{
			var dbase = GameDatabase.Instance;
			var node_list = dbase.GetConfigNodes ("EL_DefaultStructureRecipe");
			var node = node_list.LastOrDefault ();
			if (node != null) {
				var recipe = new Recipe (node);
				if (recipe.ingredients.Count > 0) {
					ExRecipeDatabase.default_structure_recipe = recipe;
				}
			}
		}

		IEnumerator<YieldInstruction> LoadModuleRecipes ()
		{
			var dbase = GameDatabase.Instance;
			var node_list = dbase.GetConfigNodes ("EL_ModuleRecipe");
			for (int i = 0; i < node_list.Length; i++) {
				var node = node_list[i];
				string name = node.GetValue ("name");

				Type mod;
				mod = AssemblyLoader.GetClassByName(typeof(PartModule), name);
				if (mod != null) {
					var recipe_node = node.GetNode ("Resources");
					var recipe = new Recipe (recipe_node);
					print ("[EL ModuleRecipe] " + name);
					ExRecipeDatabase.module_recipes[name] = recipe;
				} else {
					print ("[EL ModuleRecipe] no such module: " + name);
				}
				yield return null;
			}
		}

		IEnumerator<YieldInstruction> LoadPartRecipes()
		{
			print ("[EL Recipes] LoadPartRecipes");
			var dbase = GameDatabase.Instance;
			var configurls = dbase.GetConfigs("PART");
			foreach (var c in configurls) {
				var node = c.config;
				string name = node.GetValue("name");
				print("[EL Recipes] " + name);
				if (node.HasNode ("EL_Recipe")) {
					var recipe_node = node.GetNode ("EL_Recipe");
					var recipe = new PartRecipe (recipe_node);
					ExRecipeDatabase.part_recipes[name] = recipe;
				} else {
					var recipe = new PartRecipe ();
					ExRecipeDatabase.part_recipes[name] = recipe;
				}
				yield return null;
			}
			done = true;
		}

		IEnumerator<YieldInstruction> LoadRecipes()
		{
			LoadDefautStructureRecipe ();
			yield return StartCoroutine (LoadModuleRecipes ());
			yield return StartCoroutine (LoadPartRecipes ());
		}

		public override bool IsReady ()
		{
			return done;
		}

		public override float ProgressFraction ()
		{
			return 0;
		}

		public override string ProgressTitle ()
		{
			return  "Extraplanetary Launchpads Recipes";
		}

		public override void StartLoad()
		{
			done = false;
			StartCoroutine (LoadRecipes ());
		}
	}
}