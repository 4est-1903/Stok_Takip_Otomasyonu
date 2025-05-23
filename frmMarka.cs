using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace StokTakipOtomasyonu
{
    public partial class frmMarka : Form
    {
        public frmMarka()
        {
            InitializeComponent();
        }

        MySqlConnection baglan = new MySqlConnection("Server=localhost;Database=stok_takip;Uid=root;Pwd=root;");
        bool durum;

        private void frmMarka_Load(object sender, EventArgs e)
        {
            KategorileriYukle();
        }

        private void KategorileriYukle()
        {
            try
            {
                baglan.Open();
                MySqlCommand komut = new MySqlCommand("SELECT * FROM kategoriler", baglan);
                MySqlDataReader oku = komut.ExecuteReader();
                Dictionary<int, string> kategoriler = new Dictionary<int, string>();
                while (oku.Read())
                {
                    kategoriler.Add(Convert.ToInt32(oku["id"]), oku["kategori_adi"].ToString());
                }
                oku.Close();
                comboKategori.DataSource = new BindingSource(kategoriler, null);
                comboKategori.DisplayMember = "Value";
                comboKategori.ValueMember = "Key";
            }
            finally
            {
                baglan.Close();
            }
        }

        private void MarkaVarMi()
        {
            durum = true;
            try
            {
                baglan.Open();
                MySqlCommand komut = new MySqlCommand("SELECT * FROM markalar WHERE marka_adi = @marka AND kategori_id = @kategoriId", baglan);
                komut.Parameters.AddWithValue("@marka", txtMarka.Text.Trim());
                komut.Parameters.AddWithValue("@kategoriId", ((KeyValuePair<int, string>)comboKategori.SelectedItem).Key);
                MySqlDataReader oku = komut.ExecuteReader();
                if (oku.Read() || string.IsNullOrWhiteSpace(txtMarka.Text))
                    durum = false;
                oku.Close();
            }
            finally
            {
                baglan.Close();
            }
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            MarkaVarMi();
            if (durum)
            {
                try
                {
                    baglan.Open();
                    MySqlCommand komut = new MySqlCommand("INSERT INTO markalar (kategori_id, marka_adi) VALUES (@kategori, @marka)", baglan);
                    komut.Parameters.AddWithValue("@kategori", ((KeyValuePair<int, string>)comboKategori.SelectedItem).Key);
                    komut.Parameters.AddWithValue("@marka", txtMarka.Text.Trim());
                    komut.ExecuteNonQuery();
                    MessageBox.Show("Marka başarıyla eklendi.");
                    txtMarka.Clear();
                }
                finally
                {
                    baglan.Close();
                }
            }
            else
            {
                MessageBox.Show("Bu marka zaten mevcut veya geçersiz giriş yaptınız.", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MarkaVarMi();
            if (durum)
            {
                try
                {
                    baglan.Open();
                    MySqlCommand komut = new MySqlCommand("INSERT INTO markalar (kategori_id, marka_adi) VALUES (@kategori, @marka)", baglan);
                    komut.Parameters.AddWithValue("@kategori", ((KeyValuePair<int, string>)comboKategori.SelectedItem).Key);
                    komut.Parameters.AddWithValue("@marka", txtMarka.Text.Trim());
                    komut.ExecuteNonQuery();
                    MessageBox.Show("Marka başarıyla eklendi.");
                    txtMarka.Clear();
                }
                finally
                {
                    baglan.Close();
                }
            }
            else
            {
                MessageBox.Show("Bu marka zaten mevcut veya geçersiz giriş yaptınız.", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
