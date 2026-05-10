export class Profile {
  constructor({
    id = 0,
    userId = 0,
    name = '',
    email = '',
    username = '',
    address = '',
    age = 0,
    phoneNumber = '',
    secondEmail = '',
    photoUrl = '',
    role = ''
  } = {}) {
    this.id = id;
    this.userId = userId;
    this.name = name;
    this.email = email;
    this.username = username;
    this.address = address;
    this.age = age;
    this.phoneNumber = phoneNumber;
    this.secondEmail = secondEmail;
    this.photoUrl = photoUrl;
    this.role = role;
  }

  get displayPhotoUrl() {
    return this.photoUrl && this.photoUrl.trim() !== ''
      ? this.photoUrl
      : 'https://via.placeholder.com/40x40/10B981/FFFFFF?text=U';
  }
}
