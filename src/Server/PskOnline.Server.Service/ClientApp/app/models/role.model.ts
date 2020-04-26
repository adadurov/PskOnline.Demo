
import { Permission } from './permission.model';


export class Role {

    constructor(id: string, name: string, description?: string, permissions?: Permission[]) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.permissions = permissions;
    }

    public id: string;
    public name: string;
    public description: string;
    public usersCount: string;
    public permissions: Permission[];
}