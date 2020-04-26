import { DepTreeView } from '../viewmodels/dep-tree-view.model';
import { Department } from '../../models/department.model';
import { BranchOffice } from '../../models/branch-office.model';

export class BranchTreeView {

  constructor(branch: BranchOffice, departments: Department[]) {
    this.id = branch.id;
    this.name = branch.name;
    this.children = departments.map((dep) => new DepTreeView(dep.id, dep.name));
  }

  public id: string;

  public name: string;

  public children: DepTreeView[];
}