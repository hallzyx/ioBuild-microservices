/**
 * User Entity
 * @class
 */
export class User {
    /**
     * Constructor
     * @param id - User ID
     * @param email - User Email
     * @param role - User Role (Builder, Owner)
     */
    constructor({ id = null, email = '', role = 'Owner' }) {
        this.id = id;
        this.email = email;
        this.role = role;
    }
}
